using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DialogueSystem;
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
    private DialogueSystemDialogueSO startingNode;
    private bool isPlayerWithinInteractionDistance = false;

    #region Initialisation
    private void Awake()
    {
        InitialiseCharacterCanvas();
        characterNameCanvasGroup = characterNameText.GetComponentInParent<CanvasGroup>();
        
        characterNameCanvasGroup.alpha = 0;
        interactCanvasGroup.alpha = 0;
        startingNode = GetComponent<DialogueSystemConversation>().dialogue;
    }
    #endregion

    private void Update()
    {
        if (isPlayerWithinInteractionDistance && Input.GetButtonDown("Interact") && !DialogueController.Instance.IsConversing)
        {
            DialogueController.Instance.TriggerConversation(startingNode, this);
            interactCanvasGroup.alpha = 0;
        }
    }

    public void SetCharacterName(string _name)
    {
        characterNameText.SetText(_name);
    }

    public override float OpenCloseTransitionTime()
    {
        return base.OpenCloseTransitionTime() + characterNameTransitionTime;
    }

    protected override void OpenTransition(string _text)
    {
        Debug.Log("Open");

        LeanTween.cancel(characterNameCanvasGroup.gameObject);

        base.OpenTransition(_text);

        LeanTween.delayedCall(base.OpenCloseTransitionTime(), callback =>
        {
            LeanTween.alphaCanvas(characterNameCanvasGroup, 1, characterNameTransitionTime);
        });
    }

    protected override void CloseTransition()
    {
        Debug.Log("Close");

        LeanTween.cancel(characterNameCanvasGroup.gameObject);

        LeanTween.alphaCanvas(characterNameCanvasGroup, 0, characterNameTransitionTime);
        LeanTween.delayedCall(characterNameTransitionTime, callback =>
        {
            base.CloseTransition();
        });

        LeanTween.alphaCanvas(characterNameCanvasGroup, 1, characterNameTransitionTime);
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
}