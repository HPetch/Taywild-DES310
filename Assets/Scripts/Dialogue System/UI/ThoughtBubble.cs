using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DialogueSystem.Data;

public class ThoughtBubble : MonoBehaviour
{
    #region Variables
    public bool IsOpen { get; private set; } = false;

    public RectTransform thoughtBubbleRect;
    public RectTransform mediumBubbleRect;
    public RectTransform smallBubbleRect;
    public TextMeshProUGUI text;

    private Vector2 thoughtBubbleSize;
    private Vector2 mediumBubbleSize;
    private Vector2 smallBubbleSize;

    [SerializeField] private AudioClip thoughtBubbleClip;

    [Range(0,0.5f)]
    [SerializeField] private float smallBubbleTransitionTime = 0.1f;
    [Range(0, 0.5f)]
    [SerializeField] private float mediumBubbleTransitionTime = 0.15f;
    [Range(0, 0.5f)]
    [SerializeField] private float thoughtBubbleTransitionTime = 0.25f;

    private float sineWaveOffset = 0.0f;
    #endregion

    #region Methods
    #region Initialisation
    private void Awake()
    {
        thoughtBubbleSize = thoughtBubbleRect.sizeDelta;
        thoughtBubbleRect.sizeDelta = Vector2.zero;

        mediumBubbleSize = mediumBubbleRect.sizeDelta;
        mediumBubbleRect.sizeDelta = Vector2.zero;

        smallBubbleSize = smallBubbleRect.sizeDelta;
        smallBubbleRect.sizeDelta = Vector2.zero;

        text.SetText("");
    }
    #endregion

    private void Update()
    {
        if (!IsOpen) return;

        float yOffset = Mathf.Sin((Time.time + sineWaveOffset) * 3) * 0.02f;
        thoughtBubbleRect.anchoredPosition += new Vector2(0, yOffset);
        mediumBubbleRect.anchoredPosition += new Vector2(0, yOffset / 2);
        smallBubbleRect.anchoredPosition += new Vector2(0, yOffset / 4);
    }

    public void TransitionIn(DialogueSystemDialogueChoiceData _choiceData)
    {
        IsOpen = true;
        sineWaveOffset = Random.Range(0f, 1f);

        CancelLeanTween();

        text.SetText(_choiceData.Text);

        LeanTween.size(smallBubbleRect, smallBubbleSize, smallBubbleTransitionTime).setOnComplete(callback =>
        {
            LeanTween.size(mediumBubbleRect, mediumBubbleSize, mediumBubbleTransitionTime).setOnComplete(callback =>
            {
                LeanTween.size(thoughtBubbleRect, thoughtBubbleSize, thoughtBubbleTransitionTime);
                AudioController.Instance.PlaySound(thoughtBubbleClip, true);
            });
        });
    }

    public void TransitionOut()
    {
        IsOpen = false;

        CancelLeanTween();

        LeanTween.size(smallBubbleRect, Vector2.zero, smallBubbleTransitionTime).setOnComplete(callback =>
        {
            //AudioController.Instance.PlaySound();
            LeanTween.size(mediumBubbleRect, Vector2.zero, mediumBubbleTransitionTime).setOnComplete(callback =>
            {
                LeanTween.size(thoughtBubbleRect, Vector2.zero, thoughtBubbleTransitionTime);
            });
        });
    }

    #region Utility
    void CancelLeanTween()
    {
        LeanTween.cancel(smallBubbleRect);
        LeanTween.cancel(mediumBubbleRect);
        LeanTween.cancel(thoughtBubbleRect);
    }

    public float GetTransitionTime() { return smallBubbleTransitionTime + mediumBubbleTransitionTime + thoughtBubbleTransitionTime; }
    #endregion
    #endregion
}