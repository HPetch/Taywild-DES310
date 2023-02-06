using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableCharacters : MonoBehaviour
{
    [SerializeField] GameObject canvas;

    #region Initialisation
    private void Awake()
    {
        canvas.SetActive(false);
    }
    #endregion

    #region Collision
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canvas.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canvas.SetActive(false);
        }
    }
    #endregion
}