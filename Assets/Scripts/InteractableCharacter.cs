using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableCharacter : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    [SerializeField] private Conversation conversation;

    private bool isPlayerWithinInteractionDistance = false;

    #region Initialisation
    private void Awake()
    {
        canvas.SetActive(false);
    }
    #endregion

    private void Update()
    {
        if (isPlayerWithinInteractionDistance && Input.GetButtonDown("Interact") && !DialogueController.Instance.IsConversing)
        {
            DialogueController.Instance.TriggerConversation(conversation);
        }
    }

    #region Collision
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canvas.SetActive(true);
            isPlayerWithinInteractionDistance = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canvas.SetActive(false);
            isPlayerWithinInteractionDistance = false;
        }
    }
    #endregion
}