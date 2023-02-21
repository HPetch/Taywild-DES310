using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableCharacters : MonoBehaviour
{
    [SerializeField] GameObject canvas;

    private bool isPlayerWithinInteractionDistance = false;

    #region Initialisation
    private void Awake()
    {
        canvas.SetActive(false);
    }
    #endregion

    private void Update()
    {
        if (isPlayerWithinInteractionDistance && Input.GetButtonDown("Interact"))
        {

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