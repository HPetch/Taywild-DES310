using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSign : MonoBehaviour
{
    #region Variables
    [SerializeField] private RectTransform tutorialJournal;

    [SerializeField] private GameObject leftPage;
    [SerializeField] private GameObject rightPage;

    [Range(-750f,0f)][SerializeField] private float heightOffset = 0f;
    [SerializeField] private LeanTweenType easeIn = LeanTweenType.linear;
    [SerializeField] private LeanTweenType easeOut = LeanTweenType.linear;

    private static float tutorialJournalTransitionTime = 0.3f;
    #endregion

    #region Methods
    #region Initialisation
    private void Awake()
    {
        leftPage.SetActive(false);
        rightPage.SetActive(false);
    }
    #endregion

    #region Collision
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CancelLeanTween();

            leftPage.SetActive(true);
            rightPage.SetActive(true);
            LeanTween.moveY(tutorialJournal, heightOffset, tutorialJournalTransitionTime).setEase(easeIn);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CancelLeanTween();

            LeanTween.moveY(tutorialJournal, -1000, tutorialJournalTransitionTime).setEase(easeOut).setOnComplete(callback =>
            {
                leftPage.SetActive(false);
                rightPage.SetActive(false);
            });
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