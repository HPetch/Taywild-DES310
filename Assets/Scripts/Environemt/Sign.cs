using UnityEngine;
using TMPro;

public class Sign : MonoBehaviour
{
    #region Variables
    private CanvasGroup canvasGroup = null;
    private bool isVisible;
    #endregion

    #region Methods
    #region Initialisation
    private void Awake()
    {
        canvasGroup = GetComponentInChildren<CanvasGroup>();
        canvasGroup.alpha = 0;
    }
    #endregion

    #region Collision
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isVisible)
        {
            isVisible = true;
            LeanTween.cancel(gameObject);
            LeanTween.alphaCanvas(canvasGroup, 1, 0.15f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isVisible)
        {
            isVisible = false;
            LeanTween.cancel(gameObject);
            LeanTween.alphaCanvas(canvasGroup, 0, 0.15f);
        }
    }

    #endregion
    #endregion
}