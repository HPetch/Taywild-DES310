using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DialogueSystem.Data;

public class ThoughtBubble : MonoBehaviour
{
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

    public void TransitionIn(DialogueSystemDialogueChoiceData _choiceData)
    {
        IsOpen = true;

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

    void CancelLeanTween()
    {
        LeanTween.cancel(smallBubbleRect);
        LeanTween.cancel(mediumBubbleRect);
        LeanTween.cancel(thoughtBubbleRect);
    }

    public float GetTransitionTime() { return smallBubbleTransitionTime + mediumBubbleTransitionTime + thoughtBubbleTransitionTime; }
}