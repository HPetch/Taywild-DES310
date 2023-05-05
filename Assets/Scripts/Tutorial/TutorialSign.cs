using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSign : MonoBehaviour
{
    #region Variables
    [SerializeField] private RectTransform tutorialJournal;

    [SerializeField] private GameObject leftPage;
    [SerializeField] private GameObject rightPage;

    [SerializeField] private CanvasGroup interactCanvasGroup = null;

    [Range(-750f,75f)][SerializeField] private float heightOffset = 0f;

    private static readonly float tutorialJournalTransitionTime = 0.3f;

    private bool isTutorialJournalOpen = false;
    private bool isPlayerWithinInteractionDistance = false;
    #endregion

    #region Methods
    #region Initialisation
    private void Awake()
    {
        leftPage.SetActive(false);
        rightPage.SetActive(false);

        interactCanvasGroup.alpha = 0;
    }
    #endregion

    private void Update()
    {
        if (!isPlayerWithinInteractionDistance) return;

        if (Input.GetButtonDown("Interact"))
        {
            if (isTutorialJournalOpen) HideJournal();
            else ShowJournal();
        }
    }

    private void ShowJournal()
    {
        isTutorialJournalOpen = true;

        CancelLeanTween();

        leftPage.SetActive(true);
        rightPage.SetActive(true);
        LeanTween.moveY(tutorialJournal, heightOffset, tutorialJournalTransitionTime).setEase(LeanTweenType.linear);
    }

    private void HideJournal()
    {
        isTutorialJournalOpen = false;
        CancelLeanTween();

        LeanTween.moveY(tutorialJournal, -1000, tutorialJournalTransitionTime).setEase(LeanTweenType.linear).setOnComplete(callback =>
        {
            leftPage.SetActive(false);
            rightPage.SetActive(false);
        });
    }

    #region Collision
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            LeanTween.alphaCanvas(interactCanvasGroup, 1, 0.15f);
            isPlayerWithinInteractionDistance = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            LeanTween.alphaCanvas(interactCanvasGroup, 0, 0.15f);
            isPlayerWithinInteractionDistance = false;

            if (isTutorialJournalOpen) HideJournal();
        }
    }
    #endregion

    #region Utility
    private void CancelLeanTween()
    {
        LeanTween.cancel(tutorialJournal.gameObject);
    }
    #endregion
    #endregion
}