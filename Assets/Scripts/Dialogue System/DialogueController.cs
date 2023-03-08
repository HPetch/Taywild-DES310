// Dialogue Controller
// Handles the conversation events, a conversation can be triggered by another class by accessing the Dialogue Controller Instance and calling the public function TriggerConversation
// 
// 
// When a conversation starts or ends an event is fired alerting the other systems, primarily the objective system

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DialogueSystem.ScriptableObjects;
using DialogueSystem.Types;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    #region Events
    public event Action OnConversationStart;
    public event Action OnConversationEnd;
    #endregion

    #region Variables
    public bool IsConversing { get; private set; } = false;

    private DialogueSystemDialogueSO dialogueNode = null;
    private InteractableCharacter character = null;
    private CharacterCanvas currentDialogueCanvas = null;

    // Coroutines need to be referenced so they can be stopped prematurly in case of a skip
    private Coroutine textType = null;

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
    private bool canDisplayNext = false;

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
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }
    #endregion

    /// <summary>
    /// Each frame if a dialogue is in progress check for player input
    /// </summary>
    private void Update()
    {
        if (GameStateController.Instance.GameState != GameStateController.GameStates.DIALOGUE) return;

        if (!canDisplayNext) return;

        // If the player inputed continue the conversation
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0) || Input.GetButtonDown("Interact")) DisplayNext();
        
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) DisplayNext(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) DisplayNext(1);
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) DisplayNext(2);
    }

    /// <summary>
    /// Starts a new conversation
    /// </summary>
    public void TriggerConversation(DialogueSystemDialogueSO _startingNode, InteractableCharacter _character)
    {
        if (IsConversing) return;

        IsConversing = true;        
        character = _character;

        OnConversationStart?.Invoke();

        StartCoroutine(StartConversationDelay(_startingNode));
    }

    private IEnumerator StartConversationDelay(DialogueSystemDialogueSO _startingNode)
    {
        yield return new WaitForSeconds(1.0f);
        ComputeNode(_startingNode);
    }

    private void ComputeNode(DialogueSystemDialogueSO _node)
    {
        canDisplayNext = false;

        if (_node == null)
        {
            EndConversation();
            return;
        }

        switch (_node.NodeType)
        {
            case DialogueSystem.Types.NodeTypes.Dialogue:
                dialogueNode = _node;

                if (IsPlayerTalking)
                {
                    character.Hide();
                    PlayerDialogueController.Instance.Show(dialogueNode.Text);

                    currentDialogueCanvas = PlayerDialogueController.Instance;
                }
                else
                {
                    character.Show(dialogueNode.Text);
                    PlayerDialogueController.Instance.Hide();

                    currentDialogueCanvas = character;
                }

                textType = StartCoroutine(TypeSentence(dialogueNode.Text));
                return;

            case DialogueSystem.Types.NodeTypes.Audio:
                if (_node.Choices.Count == 0) EndConversation();
                else ComputeNode(_node.Choices[0].NextDialogue);
                return;

            case DialogueSystem.Types.NodeTypes.Edge:
                if (_node.Choices.Count == 0) EndConversation();
                else ComputeNode(_node.Choices[0].NextDialogue);
                return;

            case DialogueSystem.Types.NodeTypes.Delay:
                if (_node.Choices.Count == 0) EndConversation();
                else ComputeNode(_node.Choices[0].NextDialogue);
                return;

            default:
                return;
        }
    }

    // Displays the next conversation event
    private void DisplayNext(int _buttonIndex = -1)
    {
        // If the text type is not complete, stop that coroutine and display they final result
        if (textType != null)
        {
            StopCoroutine(textType);
            textType = null;
            
            currentDialogueCanvas.SetText(dialogueNode.Text);

            if(dialogueNode.DialogueType == DialogueTypes.MultipleChoice) ShowBranchButtons();
            return;
        }

        // If there is no conversation left, end the conversation
        if (dialogueNode.Choices.Count == 0)
        {
            EndConversation();
            return;
        }

        // If the node is a Branch
        if (dialogueNode.DialogueType == DialogueTypes.MultipleChoice)
        {
            // If a player option was selected
            if (_buttonIndex >= 0 && _buttonIndex < dialogueNode.Choices.Count)
            {
                HideBranchButtons();

                // Compute the next node
                ComputeNode(dialogueNode.Choices[_buttonIndex].NextDialogue);
            }

            // Else the player has pressed anykey, but as it's a branch they have to select an option
            return;
        }

        ComputeNode(dialogueNode.Choices[0].NextDialogue);
    }

    /// <summary>
    /// Text-Type coroutine
    /// </summary>
    private IEnumerator TypeSentence(string _sentence)
    {
        // Set text field to blank      
        textTypeString = "";
        currentDialogueCanvas.ClearText();

        // Reset TextType Delay to the default delay (incase it was changed in a link)
        currentTextTypeDelay = TextTypeDelay;

        yield return new WaitForSeconds(currentDialogueCanvas.SizeTransitionTime);
        canDisplayNext = true;

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

                    // If the Short Wait Link is used in Rich text, Set the Wait time to short.
                    if (_sentence.Substring(letterIndex + 7, 10) == "wait_short") textTypeWaitTime = shortWait;
                    // If the long Wait Link is used in Rich text, Set the Wait time to long.
                    if (_sentence.Substring(letterIndex + 7, 9) == "wait_long") textTypeWaitTime = longWait;
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

            // If a link has been started, cap the link
            if (linkStarted && !richText) currentDialogueCanvas.SetText(textTypeString + "</link>");
            else currentDialogueCanvas.SetText(textTypeString);
            
            // If there is a TextTypeWaitTime set, then wait.
            if (textTypeWaitTime > 0f)
            {
                Debug.Log("Wait");
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

        if(dialogueNode.DialogueType == DialogueTypes.MultipleChoice) ShowBranchButtons();

        // Sets coroutine to null, to track when it's finished
        textType = null;
    }

    private void ShowBranchButtons()
    {

    }

    private void HideBranchButtons()
    {

    }

    /// <summary>
    /// Ends the current conversation.
    /// </summary>
    private void EndConversation()
    {
        IsConversing = false;
        textType = null;
        dialogueNode = null;

        OnConversationEnd?.Invoke();
    }

    public void BranchButton(int _buttonID)
    {
        DisplayNext(_buttonID - 1);
    }

    public void SetTypeSpeed(float textTypeDelay)
    {
        currentTextTypeDelay = textTypeDelay;
    }

    #region Utility
    private bool IsPlayerTalking { get { return dialogueNode.Character.CharacterName == "Player"; } }
    #endregion
    #endregion
}