// Dialogue Controller
// Handles the conversation events, a conversation can be triggered by another class by accessing the Dialogue Controller Instance and calling the public function TriggerConversation
// A conversation is a collection of multiple conversation events that are computed in a sequence
// If multiple converstaions are triggered they join a queue, they are processed in the order they are triggered
// When a conversation starts or ends an event is fired alerting the other systems, primarily the objective system

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    #region Events
    public event Action<Conversation> OnConversationStart;
    public event Action<Conversation> OnConversationEnd;
    #endregion

    #region public Variables
    public bool IsConversing { get; private set; } = false;
    #endregion

    #region Private Variables
    [field: SerializeField] Conversation TestConversation { get; set; }

    private Queue<ConversationEvent> conversationEventQueue = new Queue<ConversationEvent>();
    private Queue<Conversation> conversationQueue = new Queue<Conversation>();

    [SerializeField] private  ConversationUITemplate[] conversationUITemplates;
    private ConversationUITemplate uiTemplate;

    private Conversation conversation = null;
    private ConversationEvent conversationEvent = null;

    // Cortoutines need to be referenced so they can be stopped prematurly in case of a skip
    private Coroutine textType = null;
    private Coroutine changeCharacter = null;

    private string textTypeString = "";

    [field: Tooltip("Delay between each char in the TextType Coroutine")]
    [field: Range(0,0.25f)]
    [field: SerializeField] private float TextTypeDelay { get; set; } = 0.01f;

    [Tooltip("Delay when a period is used")]    
    [field: Range(0,0.25f)]
    [field: SerializeField] private float TextTypePeriodDelay { get; set; } = 0.05f;

    [Tooltip("Delay when a comma is used")]
    [field: Range(0, 0.25f)]
    [field: SerializeField] private float TextTypeCommaDelay { get; set; } = 0.05f;

    [Tooltip("Delay when a colon is used")]
    [field: Range(0, 0.25f)]
    [field: SerializeField] private float TextTypeColonDelay { get; set; } = 0.05f;

    [Tooltip("Delay when a semi-colon is used")]
    [field: Range(0, 0.25f)]
    [field: SerializeField] private float TextTypeSemiColonDelay { get; set; } = 0.05f;

    // Tracks wether the current char is rich text or not (TextTyper)
    private bool richText = false;

    private bool linkStarted = false;
    #endregion

    #region Functions
    #region Initialisation
    // Awake is only used for setting controller instances and referencing components
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        TriggerConversation(TestConversation);
    }
    #endregion

    /// <summary>
    /// Each frame if a dialogue is in progress check for player input
    /// </summary>
    private void Update()
    {
        if (GameStateController.Instance.GameState != GameStateController.GameStates.DIALOGUE) return;

        // If the player inputed continue the conversation
        if (Input.GetMouseButtonDown(0)) DisplayNext();
    }

    /// <summary>
    /// Starts a new conversation
    /// </summary>
    public void TriggerConversation(Conversation _conversation)
    {
        // If there is already a conversation ongoing add the triggered conversation to the queue and return
        if (IsConversing)
        {
            conversationQueue.Enqueue(_conversation);
            return;
        }

        conversation = _conversation;
        // Queue each conversation event so they can be processed
        conversationEventQueue.Clear();

        StartCoroutine(StartConversation());
    }

    private IEnumerator StartConversation()
    {
        IsConversing = true;

        yield return new WaitForSeconds(conversation.ConversationStartDelay);

        // Limit player inputs while a dialogue sequence is playing
        GameStateController.Instance.DialoguePause();

        foreach (ConversationEvent convoEvent in conversation.ConversationEvents) conversationEventQueue.Enqueue(convoEvent);
        OnConversationStart?.Invoke(conversation);

        changeCharacter = StartCoroutine(ChangeCharacter(false));
    }

    // Displays the next conversation event
    private void DisplayNext()
    {
        // If the character changing animation is not complete, return and wait (when switched to tween this will skip the transition)
        if (changeCharacter != null) { return; }

        // If the text type is not complete, stop that coroutine and display they final result
        if (textType != null)
        {
            StopCoroutine(textType);
            textType = null;
            uiTemplate.TextField.text = conversationEvent.Text;
            return;
        }

        // If there is no conversation left, end the conversation
        if (conversationEventQueue.Count == 0)
        {
            StartCoroutine(EndConversation());
            return;
        }

        // else, if the next conversation event is a different character than the current one, change the character
        if (conversationEvent.Character != conversationEventQueue.Peek().Character || 
            conversationEvent.UITemplate != conversationEventQueue.Peek().UITemplate)
        {
            changeCharacter = StartCoroutine(ChangeCharacter(true));
            return;
        }

        // else, remove the current event from the queue, and start typing the next one
        conversationEvent = conversationEventQueue.Dequeue();

        textType = StartCoroutine(TypeSentance(conversationEvent.Text));
    }

    /// <summary>
    /// Text-Type coroutine
    /// </summary>
    private IEnumerator TypeSentance(string _sentence)
    {
        // Set text field to blank      
        textTypeString = "";
        uiTemplate.TextField.GetComponent<TextEffect>().ClearText();

        // For each character
        for (int letterIndex = 0; letterIndex < _sentence.Length; letterIndex++)
        {
            char letter = _sentence[letterIndex];

            // Add that character to the string
            textTypeString += letter;

            // If the current letter is part of some rich text we do not want to delay between the 'char's as the user won't see them
            if (letter == '<')
            {
                richText = true;
                if (_sentence.Substring(letterIndex + 1, 4) == "link")
                {
                    linkStarted = true;
                }
                else if (_sentence.Substring(letterIndex + 1, 5) == "/link")
                {
                    linkStarted = false;
                }
            }
            if (richText)
            {
                if (letter == '>')
                {
                    richText = false;
                }

                continue;
            }

            uiTemplate.TextField.SetText(textTypeString);

            if(linkStarted && !richText)
            {
                uiTemplate.TextField.text += "</link>";
            }

            // Update the text;
            uiTemplate.TextField.GetComponent<TextEffect>().UpdateText();

            switch (letter)
            {
                // If the char is a space
                case ' ':
                    // skip to next letter
                    continue;

                    // If the char is a period
                case '.':
                    // Wait for the duration of TextTypePeriodDelay
                    Debug.Log(".");
                    yield return new WaitForSeconds(TextTypePeriodDelay);
                    continue;

                // If the char is a Comma
                case ',':
                    Debug.Log(",");
                    // Wait for the duration of TextTypeCommaDelay
                    yield return new WaitForSeconds(TextTypeCommaDelay);
                    continue;

                // If the char is a Colon
                case ':':
                    Debug.Log(":");
                    // Wait for the duration of TextTypeColonDelay
                    yield return new WaitForSeconds(TextTypeColonDelay);
                    continue;

                // If the char is a Semi-Colon
                case ';':
                    Debug.Log(";");
                    // Wait for the duration of TextTypeSemiColonDelay
                    yield return new WaitForSeconds(TextTypeSemiColonDelay);
                    continue;

                    // Else for every other character use the default delay
                default:
                    yield return new WaitForSeconds(TextTypeDelay);
                    continue;
            }
        }

        // Sets coroutine to null, to track when it's finished
        textType = null;
    }

    private IEnumerator ChangeCharacter(bool _isOpen)
    {
        /*if (_isOpen)
        {
            //animator.SetTrigger("Closed");
            yield return new WaitForSeconds(0.33f);
        }*/

        conversationEvent = conversationEventQueue.Peek();
        uiTemplate = conversationUITemplates[(int)conversationEvent.UITemplate];

        uiTemplate.TextField.GetComponent<TextEffect>().ClearText();
        uiTemplate.CharacterName.SetText(conversationEvent.Character.CharacterName);
        uiTemplate.CharacterName.color = conversationEvent.Character.Colour;
        uiTemplate.CharacterPortrait.sprite = conversationEvent.Character.Portraits[0];

        //animator.SetTrigger("Open");
        yield return new WaitForSeconds(0.05f);

        changeCharacter = null;
        DisplayNext();
    }

    /// <summary>
    /// Ends the current conversation after the end conversation delay
    /// </summary>
    private IEnumerator EndConversation()
    {
        //animator.SetTrigger("Closed");

        yield return new WaitForSeconds(conversation.ConversationEndDelay);

        //if (triggeredObjective) ObjectiveController.Instance.AddObjective(triggeredObjective);

        GameStateController.Instance.DialogueUnPause();
        conversationEventQueue.Clear();
        IsConversing = false;

        textType = null;
        changeCharacter = null;

        OnConversationEnd?.Invoke(conversation);
        conversation = null;

        if (conversationQueue.Count > 0)
        {
            TriggerConversation(conversationQueue.Dequeue());
        }
    }

    public void BranchButton(int _buttonID)
    {

    }    

    public void SetTypeSpeed(float textTypeDelay)
    {
        TextTypeDelay = textTypeDelay;
    }
    #endregion
}