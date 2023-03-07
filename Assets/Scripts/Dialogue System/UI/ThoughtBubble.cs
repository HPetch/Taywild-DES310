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

        LeanTween.size(smallBubbleRect, smallBubbleSize, 0.1f)/*.setEasePunch()*/.setOnComplete(callback =>
        {
            LeanTween.size(mediumBubbleRect, mediumBubbleSize, 0.15f)/*.setEasePunch()*/.setOnComplete(callback =>
            {
                LeanTween.size(thoughtBubbleRect, thoughtBubbleSize, 0.25f)/*.setEasePunch()*/;
            });
        });
    }

    public void TransitionOut()
    {
        IsOpen = false;

        CancelLeanTween();

        LeanTween.size(smallBubbleRect, Vector2.zero, 0.05f)/*.setEasePunch()*/.setOnComplete(callback =>
        {
            LeanTween.size(mediumBubbleRect, Vector2.zero, 0.10f)/*.setEasePunch()*/.setOnComplete(callback =>
            {
                LeanTween.size(thoughtBubbleRect, Vector2.zero, 0.15f)/*.setEasePunch()*/;
            });
        });
    }

    void CancelLeanTween()
    {
        LeanTween.cancel(smallBubbleRect);
        LeanTween.cancel(mediumBubbleRect);
        LeanTween.cancel(thoughtBubbleRect);
    }
}
