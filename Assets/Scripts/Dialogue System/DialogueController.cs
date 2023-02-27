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

    #region Variables
    public bool IsConversing { get; private set; } = false;

    private Queue<ConversationEvent> conversationEventQueue = new Queue<ConversationEvent>();
    private Queue<ShallowBranchConversationEvent> branchConversationEventQueue = new Queue<ShallowBranchConversationEvent>();
    private Queue<Conversation> conversationQueue = new Queue<Conversation>();

    [SerializeField] private ConversationUITemplate[] conversationUITemplates;
    private ConversationUITemplate uiTemplate;

    private Conversation currentConversation = null;
    private ConversationEvent conversationEvent = null;
    private ShallowBranchConversationEvent branchConversationEvent = null;

    // Cortoutines need to be referenced so they can be stopped prematurly in case of a skip
    private Coroutine textType = null;
    private Coroutine changeCharacter = null;

    private string textTypeString = "";

    [Tooltip("Delay between each char in the TextType Coroutine")]
    [Range(0, 0.25f)]
    [SerializeField] public float TextTypeDelay = 0.01f;

    [Tooltip("Delay when a period is used")]
    [Range(0, 0.25f)]
    [SerializeField] private float TextTypePeriodDelay = 0.05f;

    [Tooltip("Delay when a comma is used")]
    [Range(0, 0.25f)]
    [SerializeField] private float TextTypeCommaDelay = 0.05f;

    [Tooltip("Delay when a colon is used")]
    [Range(0, 0.25f)]
    [SerializeField] private float TextTypeColonDelay = 0.05f;

    [Tooltip("Delay when a semi-colon is used")]
    [Range(0, 0.25f)]
    [SerializeField] private float TextTypeSemiColonDelay = 0.05f;

    // Tracks wether the current char is rich text or not (TextTyper)
    private bool richText = false;

    private bool linkStarted = false;

    private float textTypeWaitTime = 0;
    private float currentTextTypeDelay = 0.01f;

    [Tooltip("Wait time when <link='wait_short'> is used")]
    [Range(0, 0.5f)]
    [SerializeField] private float shortWait = 0.2f;
    [Tooltip("Wait time when <link='wait_long'> is used")]
    [Range(0, 1f)]
    [SerializeField] private float longWait = 0.5f;
    #endregion

    #region Functions
    #region Initialisation
    // Awake is only used for setting controller instances and referencing components
    private void Awake()
    {
        Instance = this;
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
    public void TriggerConversation(Conversation conversation)
    {
        // If there is already a conversation ongoing add the triggered conversation to the queue and return
        if (IsConversing)
        {
            conversationQueue.Enqueue(conversation);
            return;
        }

        currentConversation = conversation;
        // Queue each conversation event so they can be processed
        conversationEventQueue.Clear();

        StartCoroutine(StartConversation());
    }

    private IEnumerator StartConversation()
    {
        IsConversing = true;
        yield return new WaitForSeconds(currentConversation.ConversationStartDelay);

        // Limit player inputs while a dialogue sequence is playing
        GameStateController.Instance.DialoguePause();

        foreach (ConversationEvent convoEvent in currentConversation.ConversationEvents) conversationEventQueue.Enqueue(convoEvent);
        OnConversationStart?.Invoke(currentConversation);

        conversationEvent = conversationEventQueue.Dequeue();
        conversationUITemplates[(int)conversationEvent.UITemplate].canvasGroup.alpha = 1;
        ChangeCharacter(conversationEvent);
        //changeCharacter = StartCoroutine(ChangeCharacter());
    }

    // Displays the next conversation event
    private void DisplayNext(int _buttonIndex = -1)
    {
        // If the character changing animation is not complete, return and wait
        if (changeCharacter != null) return;

        // If the text type is not complete, stop that coroutine and display they final result
        if (textType != null)
        {
            StopCoroutine(textType);
            textType = null;
            uiTemplate.TextField.text = conversationEvent.Text;
            ShowBranchButtons();
            return;
        }

        // If there is no conversation left, end the conversation
        if (conversationEventQueue.Count == 0)
        {
            StartCoroutine(EndConversation());
            return;
        }

        // If the conversation event is a Branch
        if (conversationEvent.EventType == ConversationEventBase.ConversationEventTypes.BRANCH)
        {
            if (branchConversationEvent == null)
            {
                if (_buttonIndex >= 0)
                {
                    if (conversationEvent.BranchEvents[_buttonIndex].Conversation.Length == 0)
                    {
                        conversationEvent = conversationEventQueue.Dequeue();
                        ChangeCharacter(conversationEvent);
                        return;
                    }

                    branchConversationEventQueue.Clear();
                    foreach (ShallowBranchConversationEvent conversationEvent in conversationEvent.BranchEvents[_buttonIndex].Conversation)
                    {
                        branchConversationEventQueue.Enqueue(conversationEvent);
                    }

                    branchConversationEvent = branchConversationEventQueue.Dequeue();

                    ChangeCharacter(branchConversationEvent);
                    return;
                }

                // A button wasn't pressed
                return;
            }

            if (branchConversationEventQueue.Count > 0)
            {
                branchConversationEvent = branchConversationEventQueue.Dequeue();
                ChangeCharacter(branchConversationEvent);
                return;
            }
            else
            {
                branchConversationEvent = null;
                conversationEvent = conversationEventQueue.Dequeue();
                ChangeCharacter(conversationEvent);
                return;
            }
        }

        conversationEvent = conversationEventQueue.Dequeue();
        ChangeCharacter(conversationEvent);
    }

    /// <summary>
    /// Text-Type coroutine
    /// </summary>
    private IEnumerator TypeSentance(string sentence)
    {
        // Set text field to blank      
        textTypeString = "";
        uiTemplate.TextField.GetComponent<TextEffect>().ClearText();

        // Reset TextType Delay to the default delay (incase it was changed in a link)
        currentTextTypeDelay = TextTypeDelay;

        // For each character
        for (int letterIndex = 0; letterIndex < sentence.Length; letterIndex++)
        {
            char letter = sentence[letterIndex];

            // Add that character to the string
            textTypeString += letter;

            // If the current letter is part of some rich text we do not want to delay between the 'char's as the user won't see them
            if (letter == '<')
            {
                richText = true;
                if (sentence.Substring(letterIndex + 1, 4) == "link")
                {
                    linkStarted = true;

                    // If the Short Wait Link is used in Rich text, Set the Wait time to short.
                    if (sentence.Substring(letterIndex + 7, 10) == "wait_short") textTypeWaitTime = shortWait;
                    // If the long Wait Link is used in Rich text, Set the Wait time to long.
                    if (sentence.Substring(letterIndex + 7, 9) == "wait_long") textTypeWaitTime = longWait;
                }
                else if (sentence.Substring(letterIndex + 1, 5) == "/link")
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

            if (linkStarted && !richText)
            {
                uiTemplate.TextField.text += "</link>";
            }

            // Update the text.
            uiTemplate.TextField.GetComponent<TextEffect>().UpdateText();

            // If there is a TextTypeWaitTime set, then wait.
            if (textTypeWaitTime > 0f)
            {
                yield return new WaitForSeconds(textTypeWaitTime);
                // Once wait has been completed set variable to 0.
                textTypeWaitTime = 0f;
            }

            switch (letter)
            {
                // If the char is a space
                case ' ':
                    // skip to next letter
                    continue;

                // If the char is a period
                case '.':
                    // Wait for the duration of TextTypePeriodDelay
                    yield return new WaitForSeconds(TextTypePeriodDelay);
                    continue;

                // If the char is a Comma
                case ',':
                    // Wait for the duration of TextTypeCommaDelay
                    yield return new WaitForSeconds(TextTypeCommaDelay);
                    continue;

                // If the char is a Colon
                case ':':
                    // Wait for the duration of TextTypeColonDelay
                    yield return new WaitForSeconds(TextTypeColonDelay);
                    continue;

                // If the char is a Semi-Colon
                case ';':
                    // Wait for the duration of TextTypeSemiColonDelay
                    yield return new WaitForSeconds(TextTypeSemiColonDelay);
                    continue;

                // Else for every other character use the default delay
                default:
                    yield return new WaitForSeconds(currentTextTypeDelay);
                    continue;
            }
        }

        ShowBranchButtons();

        // Sets coroutine to null, to track when it's finished
        textType = null;
    }

    /*private IEnumerator ChangeCharacter()
    {
        uiTemplate = conversationUITemplates[(int)conversationEvent.UITemplate];
        HideBranchButtons();
        uiTemplate.TextField.GetComponent<TextEffect>().ClearText();
        if (branchConversationEvent != null)
        {
            uiTemplate.CharacterName.SetText(branchConversationEvent.Character.CharacterName);
            uiTemplate.CharacterName.color = branchConversationEvent.Character.Colour;
            uiTemplate.CharacterPortrait.sprite = branchConversationEvent.Character.Portraits[0];
            yield return new WaitForSeconds(0.1f);
            changeCharacter = null;
            textType = StartCoroutine(TypeSentance(branchConversationEvent.Text));
        }
        else
        {
            uiTemplate.CharacterName.SetText(conversationEvent.Character.CharacterName);
            uiTemplate.CharacterName.color = conversationEvent.Character.Colour;
            uiTemplate.CharacterPortrait.sprite = conversationEvent.Character.Portraits[0];
            yield return new WaitForSeconds(0.1f);
            changeCharacter = null;
            textType = StartCoroutine(TypeSentance(conversationEvent.Text));
        }
    }*/

    private void ChangeCharacter(ConversationEventBase _conversationEvent)
    {
        uiTemplate = conversationUITemplates[(int)_conversationEvent.UITemplate];
        HideBranchButtons();

        uiTemplate.TextField.GetComponent<TextEffect>().ClearText();

        uiTemplate.CharacterName.SetText(_conversationEvent.Character.CharacterName);
        uiTemplate.CharacterName.color = _conversationEvent.Character.Colour;
        uiTemplate.CharacterPortrait.sprite = _conversationEvent.Character.Portraits[0];

        changeCharacter = null;
        textType = StartCoroutine(TypeSentance(_conversationEvent.Text));
    }

    private void ShowBranchButtons()
    {
        if (conversationEvent.EventType != ConversationEventBase.ConversationEventTypes.BRANCH || branchConversationEvent != null) return;

        for (int button = 0; button < uiTemplate.BranchButtons.Length; button++)
        {
            if (button < conversationEvent.BranchEvents.Length)
            {
                uiTemplate.BranchButtons[button].canvasGroup.alpha = 1;
                uiTemplate.BranchButtons[button].buttonText.SetText(conversationEvent.BranchEvents[button].ButtonText);
            }
            else
            {
                uiTemplate.BranchButtons[button].canvasGroup.alpha = 0;
            }
        }
    }

    private void HideBranchButtons()
    {
        for (int button = 0; button < uiTemplate.BranchButtons.Length; button++)
        {
            uiTemplate.BranchButtons[button].canvasGroup.alpha = 0;
        }
    }

    /// <summary>
    /// Ends the current conversation after the end conversation delay
    /// </summary>
    private IEnumerator EndConversation()
    {
        yield return new WaitForSeconds(currentConversation.ConversationEndDelay);

        GameStateController.Instance.DialogueUnPause();
        conversationEventQueue.Clear();
        IsConversing = false;

        textType = null;
        changeCharacter = null;
        uiTemplate.canvasGroup.alpha = 0;

        OnConversationEnd?.Invoke(currentConversation);
        currentConversation = null;

        if (conversationQueue.Count > 0)
        {
            TriggerConversation(conversationQueue.Dequeue());
        }
    }

    public void BranchButton(int _buttonID)
    {
        DisplayNext(_buttonID - 1);
    }

    public void SetTypeSpeed(float textTypeDelay)
    {
        currentTextTypeDelay = textTypeDelay;
    }
    #endregion
}