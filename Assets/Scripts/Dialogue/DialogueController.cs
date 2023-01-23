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

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    #region Events
    public event Action<Conversation> OnConversationStart;
    public event Action<Conversation> OnConversationEnd;
    #endregion

    #region public Variables
    public Conversation CurrentConversation { get; private set; }
    public bool IsConversing { get; private set; } = false;
    #endregion

    #region Private Variables
    [SerializeField] private Image characterImage = null;    
    [SerializeField] private TextMeshProUGUI characterName = null;
    [SerializeField] private TextMeshProUGUI textField = null;

    private Queue<Speech> conversationEventQueue = new Queue<Speech>();
    private Queue<Conversation> conversationQueue = new Queue<Conversation>();
    //private Objective triggeredObjective;

    private Speech speech;

    // Cortoutines need to be referenced so they can be stopped prematurly in case of a skip
    private Coroutine textType = null;
    private Coroutine changeCharacter = null;

    // Tracks wether the current char is rich text or not (TextTyper)
    private bool richText = false;
    #endregion

    #region Functions
    #region Initialisation
    // Awake is only used for setting controller instances and referencing components
    private void Awake()
    {
        Instance = this;
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

        CurrentConversation = _conversation;

        // Queue each conversation event so they can be processed
        conversationEventQueue.Clear();

        StartCoroutine(StartConversation());
    }

    private IEnumerator StartConversation()
    {
        IsConversing = true;

        yield return new WaitForSeconds(CurrentConversation.ConversationStartDelay);

        // Limit player inputs while a dialogue sequence is playing
        GameStateController.Instance.DialoguePause();

        foreach (Speech speech in CurrentConversation.converstation) conversationEventQueue.Enqueue(speech);
        //triggeredObjective = _conversation.triggeredObjective;
        OnConversationStart?.Invoke(CurrentConversation);
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
            textField.text = speech.text;
            return;
        }

        // If there is no conversation left, end the conversation
        if (conversationEventQueue.Count == 0)
        {
            StartCoroutine(EndConversation());
            return;
        }

        // else, if the next conversation event is a different character than the current one, change the character
        if (speech.character != conversationEventQueue.Peek().character)
        {
            changeCharacter = StartCoroutine(ChangeCharacter(true));
            return;
        }

        // else, remove the current event from the queue, and start typing the next one
        speech = conversationEventQueue.Dequeue();
        textType = StartCoroutine(TypeSentance(speech.text));
    }

    // Text-Type coroutine
    private IEnumerator TypeSentance(string _sentence)
    {
        // Set text field to blank
        textField.text = "";

        // For each character
        foreach (char letter in _sentence.ToCharArray())
        {
            // Add that character to the string
            textField.text += letter;

            // If the current letter is part of some rich text we do not want to delay between the 'char's as the user won't see them
            if (letter == '<') richText = true;
            if (richText)
            {
                if (letter == '>') richText = false;
                continue;
            }

            yield return new WaitForSeconds(0.01f);
        }

        // Sets coroutine to null, to track when it's finished
        textType = null;
    }

    private IEnumerator ChangeCharacter(bool _isOpen)
    {
        if (_isOpen)
        {
            //animator.SetTrigger("Closed");
            yield return new WaitForSeconds(0.33f);
        }

        speech = conversationEventQueue.Peek();

        characterName.text = speech.character.characterName;
        characterName.color = speech.character.color;
        characterImage.sprite = speech.character.image;
        textField.text = "";

        //animator.SetTrigger("Open");
        yield return new WaitForSeconds(0.33f);

        changeCharacter = null;
        DisplayNext();
    }

    // Ends the current conversation after the end conversation delay
    private IEnumerator EndConversation()
    {
        //animator.SetTrigger("Closed");

        yield return new WaitForSeconds(CurrentConversation.ConversationEndDelay);

        //if (triggeredObjective) ObjectiveController.Instance.AddObjective(triggeredObjective);

        GameStateController.Instance.DialogueUnPause();
        conversationEventQueue.Clear();
        IsConversing = false;

        textType = null;
        changeCharacter = null;

        OnConversationEnd?.Invoke(CurrentConversation);
        CurrentConversation = null;

        if (conversationQueue.Count > 0)
        {
            Conversation conversation = conversationQueue.Dequeue();
            TriggerConversation(conversation);
        }
    }
    #endregion
}