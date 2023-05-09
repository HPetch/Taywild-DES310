using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingTrigger : MonoBehaviour
{
    #region Variables
    [SerializeField] private RectTransform journal;
    private static readonly float journalTransitionTime = 0.3f;

    private bool isJournalOpen = false;
    private bool triggered = false;
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
        if (triggered) return;

        foreach (ObjectiveController.Quest quest in ObjectiveController.Instance.Quests)
        {
            if (Input.GetKeyDown(KeyCode.P)) quest.State = DialogueSystem.Types.QuestStates.Completed;

            if (quest.State != DialogueSystem.Types.QuestStates.Completed)
            {
                return;
            }
        }

        triggered = true;
        ShowJournal();
    }

    private void ShowJournal()
    {
        if (isJournalOpen) return;

        isJournalOpen = true;
        CancelLeanTween();

        journal.gameObject.SetActive(true);
        LeanTween.moveY(journal, 0, journalTransitionTime).setEase(LeanTweenType.linear);
    }

    public void HideJournal()
    {
        if (!isJournalOpen) return;

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
