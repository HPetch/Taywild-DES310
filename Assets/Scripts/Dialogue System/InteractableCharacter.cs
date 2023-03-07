using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        //LeanTween.cancel(characterNameCanvasGroup.gameObject);

        base.OpenTransition(_text);

        LeanTween.delayedCall(base.OpenCloseTransitionTime(), callback =>
        {
            LeanTween.alphaCanvas(characterNameCanvasGroup, 1, characterNameTransitionTime);
        });
    }

    public override void CloseTransition()
    {
        //LeanTween.cancel(characterNameCanvasGroup.gameObject);

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
}