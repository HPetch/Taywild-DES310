using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueSystem;
using DialogueSystem.ScriptableObjects;

public class InteractableCharacter : CharacterCanvas
{
    [Header("Interaction Settings")]
    [SerializeField] private CanvasGroup interactCanvasGroup;

    private DialogueSystemDialogueSO startingNode;
    private bool isPlayerWithinInteractionDistance = false;

    #region Initialisation
    private void Awake()
    {
        InitialiseCharacterCanvas(); 

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