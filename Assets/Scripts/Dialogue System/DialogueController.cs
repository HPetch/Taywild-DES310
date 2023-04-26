// Dialogue Controller
// Handles the conversation events, a conversation can be triggered by another class by accessing the Dialogue Controller Instance and calling the public function TriggerConversation
// 
// 
// When a conversation starts or ends an event is fired alerting the other systems, primarily the objective system

using System;
using UnityEngine;
using System.Collections;
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
    public InteractableCharacter Character { get; private set; } = null;
    public DialogueSystemDialogueContainerSO CurrentGraph { get; private set; } = null;

    private DialogueSystemDialogueSO dialogueNode = null;
    private CharacterCanvas currentDialogueCanvas = null;

    // Coroutines need to be referenced so they can be stopped prematurly in case of a skip
    private Coroutine textType = null;

    private string textTypeString = "";

    [Header("Audio clips")]
    [SerializeField] private AudioClip zoomInClip;

    [Header("Delays")]
    [Tooltip("Delay between each char in the TextType Coroutine")]
    [Range(0, 0.5f)]
    [SerializeField] public float startConversationDelay = 0.2f;

    [field: Tooltip("Delay between each char in the TextType Coroutine")]
    [field: Range(0, 0.25f)]
    [field: SerializeField] public float TextTypeDelay { get; private set; } = 0.01f;

    [Tooltip("Delay when a period is used")]
    [Range(0, 0.25f)]
    [SerializeField] private float textTypePeriodDelay = 0.05f;

    [Tooltip("Delay when a comma is used")]
    [Range(0, 0.25f)]
    [SerializeField] private float textTypeCommaDelay = 0.05f;

    [Tooltip("Delay when a colon is used")]
    [Range(0, 0.25f)]
    [SerializeField] private float textTypeColonDelay = 0.05f;

    [Tooltip("Delay when a semi-colon is used")]
    [Range(0, 0.25f)]
    [SerializeField] private float textTypeSemiColonDelay = 0.05f;

    // Tracks wether the current char is rich text or not (TextTyper)
    private bool richText = false;

    private bool linkStarted = false;
    private bool canDisplayNext = false;

    private float textTypeWaitTime = 0;
    private float currentTextTypeDelay = 0.01f;

    private float timeOfLastDisplayNext = 0.0f;

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
        //if (GameStateController.Instance.GameState != GameStateController.GameStates.DIALOGUE) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopCoroutine(textType);
            PlayerDialogueController.Instance.HideThoughtBubbles(0);
            PlayerDialogueController.Instance.CloseTransition();
            Character.CloseTransition();
            EndConversation();
        }

        if (!canDisplayNext) return;

        if (Input.GetMouseButtonDown(0))
        {
            // If single choice node, display next
            if (dialogueNode.DialogueType == DialogueTypes.SingleChoice) DisplayNext();
            // else if Multiple choice node, only display next to skip text or display options
            else if (dialogueNode.DialogueType == DialogueTypes.MultipleChoice)
            {
                // Skip text type
                if (textType != null) DisplayNext();
                // If text type is done, and thought bubles are not open, 
                else if (!PlayerDialogueController.Instance.IsThoughtBubblesOpen)
                {
                    // Close charater speech bubble
                    Character.CloseTransition();
                    // Show player options
                    PlayerDialogueController.Instance.ShowThoughtBubbles(dialogueNode);
                }
            }
        }
        
        else if ((Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) && dialogueNode.DialogueType == DialogueTypes.MultipleChoice && PlayerDialogueController.Instance.ThoughtBubbleTransitionComplete) DisplayNext(1);
        else if ((Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) && dialogueNode.DialogueType == DialogueTypes.MultipleChoice && PlayerDialogueController.Instance.ThoughtBubbleTransitionComplete) DisplayNext(2);
        else if ((Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) && dialogueNode.DialogueType == DialogueTypes.MultipleChoice && PlayerDialogueController.Instance.ThoughtBubbleTransitionComplete) DisplayNext(3);
    }

    /// <summary>
    /// Starts a new conversation
    /// </summary>
    public void TriggerConversation(DialogueSystemDialogueContainerSO _graph, InteractableCharacter _character)
    {
        IsConversing = true;        
        Character = _character;
        CurrentGraph = _graph;

        ////Audio on dialogue zoom here
        AudioController.Instance.PlaySound(zoomInClip,true);
        OnConversationStart?.Invoke();

        StartCoroutine(StartConversationDelay(_graph.StartingNode));
    }

    private IEnumerator StartConversationDelay(DialogueSystemDialogueSO _startingNode)
    {
        yield return new WaitForSeconds(startConversationDelay);
        StartCoroutine(ComputeNode(_startingNode));
    }

    private IEnumerator ComputeNode(DialogueSystemDialogueSO _node, float _delay = 0)
    {
        if (_node == null)
        {
            EndConversation();
            yield break;
        }

        canDisplayNext = false;

        yield return new WaitForSeconds(_delay);

        switch (_node.NodeType)
        {
            // If the node is a DialogueNode 
            case NodeTypes.Dialogue:               
                dialogueNode = _node;
                bool resize = true;

                // If the player is talking in the new dialogueNode and the their speech bubble is closed
                if (IsPlayerTalking && !PlayerDialogueController.Instance.IsOpen)
                {
                    // If the character speech bubble is open
                    if (Character.IsOpen)
                    {
                        // Close the character speech bubble
                        Character.CloseTransition();
                        // Wait for it to close
                        yield return new WaitForSeconds(Character.ResizeTransitionTime());
                    }

                    // Open player speech bubble
                    PlayerDialogueController.Instance.OpenTransition(dialogueNode.Text);
                    // Change the current active speech canvas to the player's
                    currentDialogueCanvas = PlayerDialogueController.Instance;
                    // do not resisze as the active talker has changed
                    resize = false;
                }
                // If the character is talking and their canvas is not open
                else if (!IsPlayerTalking && !Character.IsOpen)
                {
                    // If the player speech bubble is open
                    if (PlayerDialogueController.Instance.IsOpen)
                    {
                        // Close it and wait for it to close
                        PlayerDialogueController.Instance.CloseTransition();
                        yield return new WaitForSeconds(PlayerDialogueController.Instance.ResizeTransitionTime());
                    }

                    // Open character speech bubble
                    Character.OpenTransition(dialogueNode.Text);
                    // Change the current active speech canvas to the Character's
                    currentDialogueCanvas = Character;
                    // do not resisze as the active talker has changed
                    resize = false;
                }

                // Set the active character, this is done here as opposed to at the start of the convo in case the character changes (going from ??? -> recluse)
                if(!IsPlayerTalking) Character.SetCharacter(dialogueNode.Character);

                // If the active talker has not changed, then simply resize the open speech bubble
                if (resize) currentDialogueCanvas.ResizieTransition(dialogueNode.Text);

                textType = StartCoroutine(TypeSentence(dialogueNode.Text, resize));
                break;
                
            case NodeTypes.Audio:
                StartCoroutine(ComputeNode(_node.Choices[0].NextDialogue));
                yield break;

            case NodeTypes.Edge:
                StartCoroutine(ComputeNode(_node.Choices[0].NextDialogue));
                yield break;

            case NodeTypes.Delay:
                yield return new WaitForSeconds(_node.Delay);
                StartCoroutine(ComputeNode(_node.Choices[0].NextDialogue));
                yield break;
                
            case NodeTypes.GetQuest:
                QuestStates questState = ObjectiveController.Instance.GetQuest(_node.Quest).State;
                StartCoroutine(ComputeNode(_node.Choices[(int)questState].NextDialogue));
                yield break;

            case NodeTypes.SetQuest:
                ObjectiveController.Quest quest = ObjectiveController.Instance.GetQuest(_node.Quest);
                quest.State = _node.QuestState;
                if (_node.QuestState == QuestStates.InProgress && quest.questObjectiveCompleteBeforeQuestIssued)
                {
                    quest.State = QuestStates.HandIn;
                }
                StartCoroutine(ComputeNode(_node.Choices[0].NextDialogue));
                yield break;

            case NodeTypes.Graph:
                TriggerConversation(_node.Graph, Character);
                yield break;

            default:
                yield break;
        }

        yield return null;
    }

    // Displays the next conversation event
    private void DisplayNext(int _buttonIndex = 0)
    {
        if (Time.time - timeOfLastDisplayNext < 0.2f) return;

        timeOfLastDisplayNext = Time.time;

        // If the text type is not complete, stop that coroutine and display they final result
        if (textType != null)
        {
            StopCoroutine(textType);
            textType = null;
            canDisplayNext = true;

            currentDialogueCanvas.SetText(dialogueNode.Text);
            currentDialogueCanvas.ShowContinueIndicator();

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
            if (_buttonIndex > 0)
            {
                // If option 3 was selected and there's only 2 options, assume they meant option 2 (as the second option is in the 3rd option spot)
                _buttonIndex = dialogueNode.Choices.Count == 2 && _buttonIndex == 3 ? 2 : _buttonIndex;
                // If option 2 was selected and there's only 1 option, assume they meant option 1 (as the only option is in the 2nd option spot)
                _buttonIndex = dialogueNode.Choices.Count == 1 && _buttonIndex == 2 || _buttonIndex == 1 ? 1 : _buttonIndex;

                PlayerDialogueController.Instance.HideThoughtBubbles(_buttonIndex - 1);

                // Compute the next node
                StartCoroutine(ComputeNode(dialogueNode.Choices[_buttonIndex - 1].NextDialogue, 0.3f));
            }

            // Else the player has pressed anykey, but as it's a branch they have to select an option
            return;
        }

        StartCoroutine(ComputeNode(dialogueNode.Choices[0].NextDialogue));
    }

    /// <summary>
    /// Text-Type coroutine
    /// </summary>
    private IEnumerator TypeSentence(string _sentence, bool _resize)
    {
        // Set text field to blank      
        textTypeString = "";
        currentDialogueCanvas.ClearText();

        // Reset TextType Delay to the default delay (incase it was changed in a link)
        currentTextTypeDelay = TextTypeDelay;

        float transitionTime = _resize ? currentDialogueCanvas.ResizeTransitionTime() : currentDialogueCanvas.OpenCloseTransitionTime();

        yield return new WaitForSeconds(transitionTime);
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
                if ((letterIndex + 5 <= _sentence.Length) && _sentence.Substring(letterIndex + 1, 4) == "link")
                {
                    linkStarted = true;

                    // If the Short Wait Link is used in Rich text, Set the Wait time to short.
                    if ((letterIndex + 17 <= _sentence.Length) && _sentence.Substring(letterIndex + 7, 10) == "wait_short") textTypeWaitTime = shortWait;
                    // If the long Wait Link is used in Rich text, Set the Wait time to long.
                    if ((letterIndex + 16 <= _sentence.Length) && _sentence.Substring(letterIndex + 7, 9) == "wait_long") textTypeWaitTime = longWait;
                }
                else if ((letterIndex + 6 <= _sentence.Length) && _sentence.Substring(letterIndex + 1, 5) == "/link")
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
                    yield return new WaitForSeconds(textTypePeriodDelay);
                    continue;

                // If the char is a Comma
                case ',':
                    // Wait for the duration of TextTypeCommaDelay
                    yield return new WaitForSeconds(textTypeCommaDelay);
                    continue;

                // If the char is a Colon
                case ':':
                    // Wait for the duration of TextTypeColonDelay
                    yield return new WaitForSeconds(textTypeColonDelay);
                    continue;

                // If the char is a Semi-Colon
                case ';':
                    // Wait for the duration of TextTypeSemiColonDelay
                    yield return new WaitForSeconds(textTypeSemiColonDelay);
                    continue;

                // Else for every other character use the default delay
                default:

                    //Play typewriter sound here
                    if (dialogueNode.Character.Voice.name == "SFX_Talk_Misc")
                        AudioController.Instance.PlaySound(dialogueNode.Character.Voice, false);
                    else
                        AudioController.Instance.PlaySound(dialogueNode.Character.Voice, true);

                    yield return new WaitForSeconds(currentTextTypeDelay);
                    continue;
            }
        }

        currentDialogueCanvas.ShowContinueIndicator();

        // Sets coroutine to null, to track when it's finished
        textType = null;
    }

    /// <summary>
    /// Ends the current conversation.
    /// </summary>
    private void EndConversation()
    {
        IsConversing = false;

        if (textType != null)
        {
            StopCoroutine(textType);
            textType = null;
        }

        OnConversationEnd?.Invoke();
    }

    public void BranchButton(int _buttonID)
    {
        DisplayNext(_buttonID);
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