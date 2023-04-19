using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DialogueSystem.ScriptableObjects;

public class InteractableCharacter : CharacterCanvas
{
    [Header("Dialogue References")]
    [SerializeField] private TextMeshProUGUI characterNameText;

    [Header("Dialogue Transition Settings")]
    [Range(0, 1)]
    [SerializeField] private float characterNameTransitionTime = 0.2f;

    [Header("Interaction Settings")]
    [SerializeField] private CanvasGroup interactCanvasGroup;

    private CanvasGroup characterNameCanvasGroup;
    public DialogueSystemDialogueContainerSO dialogueGraph;

    private bool isPlayerWithinInteractionDistance = false;

    [field: Range(4, 15)]
    [field: SerializeField] public float ConversationZoomLevel { get; private set; } = 5;

    #region Initialisation
    private void Awake()
    {
        InitialiseCharacterCanvas();
        characterNameCanvasGroup = characterNameText.GetComponentInParent<CanvasGroup>();
        
        characterNameCanvasGroup.alpha = 0;
        interactCanvasGroup.alpha = 0;
    }
    #endregion

    private void Update()
    {
        if (isPlayerWithinInteractionDistance && Input.GetButtonDown("Interact") && !DialogueController.Instance.IsConversing && !DecorationController.Instance.isEditMode)
        {
            DialogueController.Instance.TriggerConversation(dialogueGraph, this);
            interactCanvasGroup.alpha = 0;
        }
    }

    public void SetCharacter(DialogueCharacter _character)
    {
        if (string.IsNullOrEmpty(_character.CharacterName))
        {
            characterNameCanvasGroup.gameObject.SetActive(false);
        }
        else
        {
            characterNameCanvasGroup.gameObject.SetActive(true);
            characterNameText.SetText(_character.CharacterName);
            characterNameCanvasGroup.GetComponent<Image>().color = _character.Colour;
        }
    }

    public override float OpenCloseTransitionTime()
    {
        return base.OpenCloseTransitionTime() + characterNameTransitionTime;
    }

    public override void OpenTransition(string _text)
    {      
        base.OpenTransition(_text);

        LeanTween.delayedCall(base.OpenCloseTransitionTime(), callback =>
        {
            LeanTween.alphaCanvas(characterNameCanvasGroup, 1, characterNameTransitionTime);
        });
    }

    public override void CloseTransition()
    {
        LeanTween.alphaCanvas(characterNameCanvasGroup, 0, characterNameTransitionTime);

        LeanTween.delayedCall(characterNameTransitionTime, callback =>
        {
            base.CloseTransition();
        });
    }

    #region Collision
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !DialogueController.Instance.IsConversing)
        {
            interactCanvasGroup.alpha = 1;
            isPlayerWithinInteractionDistance = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !DialogueController.Instance.IsConversing)
        {
            interactCanvasGroup.alpha = 0;
            isPlayerWithinInteractionDistance = false;
        }
    }
    #endregion

    #region Utility
    protected override void CancelLeanTween()
    {
        base.CancelLeanTween();
        LeanTween.cancel(characterNameCanvasGroup.gameObject);
    }
    #endregion
}