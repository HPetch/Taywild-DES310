// Dialogue Controller
// Handles the conversation events, a conversation can be triggered by another class by accessing the Dialogue Controller Instance and calling the public function TriggerConversation
// A conversation is a collection of multiple conversation events that are computed in a sequence
// If multiple converstaions are triggered they join a queue, they are processed in the order they are triggered
// When a conversation starts or ends an event is fired alerting the other systems, primarily the objective system

using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class ConversationUITemplate
{
    public enum ConversationUITemplates { BOTTOM_RIGHT, BOTTOM_MIDDLE, TOP_RIGHT }

    [field: SerializeField] public Image CharacterImage { get; private set; } = null;
    [field: SerializeField] public TextMeshProUGUI CharacterName { get; private set; } = null;
    [field: SerializeField] public TextMeshProUGUI TextField { get; private set; } = null;
}

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    #region Events
    public event Action<Conversation> OnConversationStart;
    public event Action<Conversation> OnConversationEnd;
    #endregion

    #region public Variables
    public bool IsConversing { get; private set; } = false;
    public ConversationUITemplate UITemplate { get; private set; }
    #endregion

    #region Private Variables

    private Queue<ConversationEvent> conversationEventQueue = new Queue<ConversationEvent>();
    private Queue<Conversation> conversationQueue = new Queue<Conversation>();

    [SerializeField] private  ConversationUITemplate[] conversationUITemplates;

    private Conversation conversation = null;
    private ConversationEvent conversationEvent = null;

    // Cortoutines need to be referenced so they can be stopped prematurly in case of a skip
    private Coroutine textType = null;
    private Coroutine changeCharacter = null;

    // Tracks wether the current char is rich text or not (TextTyper)
    private bool richText = false;
    // Tracks wether the current char is fancy text or not (TextTyper)
    private bool fancyText = false;
    // Delay between each char in the TextType Coroutine
    private float textTypeDelay = 0.01f;
    #endregion

    #region Functions
    #region Initialisation
    // Awake is only used for setting controller instances and referencing components
    private void Awake()
    {
        Instance = this;
        UITemplate = conversationUITemplates[0];
    }
    #endregion

    // Each frame if a dialogue is in progress check for player input
    private void Update()
    {
        if (GameStateController.Instance.GameState != GameStateController.GameStates.DIALOGUE) return;

        // If the player inputed continue the conversation
        if (Input.GetMouseButtonDown(0)) DisplayNext();
    }

    // Starts a new conversation
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

        // If the text type is not complete, stop that coroutine and displat they final result
        if (textType != null)
        {
            StopCoroutine(textType);
            textType = null;
            UITemplate.TextField.text = conversationEvent.Text;
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

    // Text-Type coroutine
    private IEnumerator TypeSentance(string _sentence)
    {
        // Set text field to blank

        UITemplate.TextField.text = "";

        // For each character
        foreach (char letter in _sentence.ToCharArray())
        {
            // Add that character to the string
            UITemplate.TextField.text += letter;

            // If the current letter is part of some rich text we do not want to delay between the 'char's as the user won't see them
            if (letter == '<') richText = true;
            if (richText)
            {
                if (letter == '>') richText = false;
                continue;
            }

            // If the current letter is part of some fancy text we do not want to delay between the 'char's as the user won't see them
            if (letter == '[') fancyText = true;
            if (fancyText)
            {
                if (letter == ']') fancyText = false;
                continue;
            }

            yield return new WaitForSeconds(textTypeDelay);
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
        UITemplate = conversationUITemplates[(int)conversationEvent.UITemplate];
        UITemplate.CharacterName.text = conversationEvent.Character.CharacterName;
        UITemplate.CharacterName.color = conversationEvent.Character.Colour;
        UITemplate.CharacterImage.sprite = conversationEvent.Character.Portraits[0];
        UITemplate.TextField.text = "";

        //animator.SetTrigger("Open");
        yield return new WaitForSeconds(0.05f);

        changeCharacter = null;
        DisplayNext();
    }

    // Ends the current conversation after the end conversation delay
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
    #endregion
}