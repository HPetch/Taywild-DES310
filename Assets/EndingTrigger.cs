using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingTrigger : MonoBehaviour
{
    #region Variables
    [SerializeField] private RectTransform journal;
    private static readonly float journalTransitionTime = 0.3f;

    private bool isJournalOpen = false;
    #endregion

    #region Methods
    #region Initialisation
    private void Awake()
    {
        journal.gameObject.SetActive(false);
    }
    #endregion

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && (isJournalOpen))
        {
            HideJournal();
        }
    }

    public void ShowJournal()
    {
        isJournalOpen = true;

        CancelLeanTween();

        journal.gameObject.SetActive(true);
        LeanTween.moveY(journal, 0, journalTransitionTime).setEase(LeanTweenType.linear);
    }

    private void HideJournal()
    {
        isJournalOpen = false;
        CancelLeanTween();

        LeanTween.moveY(journal, -1000, journalTransitionTime).setEase(LeanTweenType.linear).setOnComplete(callback =>
        {
            journal.gameObject.SetActive(false);
        });
    }
    #endregion

    #region Utility
    private void CancelLeanTween()
    {
        LeanTween.cancel(journal.gameObject);
    }
    #endregion
}
