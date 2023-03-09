using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MinMaxSlider;

public class CharacterCanvas : MonoBehaviour
{
    public bool IsOpen { get; private set; } = false;

    [Header("Dialogue References")]
    [SerializeField] private RectTransform dialogueContainer;
    [SerializeField] private RectTransform speechBubble;
    [SerializeField] private RectTransform tail;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private CanvasGroup continueIndicator;

    [Space(10)]
    [Header("Speech Box Settings")]

    [MinMaxSlider(10, 1000)]
    [SerializeField] private Vector2Int widthRange = new Vector2Int(100, 600);

    [MinMaxSlider(10, 500)]
    [SerializeField] private Vector2Int heightRange = new Vector2Int(60, 300);

    [Range(0, 2)]
    [SerializeField] private float speachBubbleFloatFrequency = 1;

    [Range(0, 5)]
    [SerializeField] private float speachBubbleFloatAmplitude = 0.5f;

    [Space(10)]
    [Header("Dialogue Transition Settings")]
    [Range(0, 1)]
    [SerializeField] private float tailTransitionTime = 0.1f;

    [Range(0, 1)]
    [SerializeField] private float speechBubbleOpenCloseTransitionTime = 0.3f;

    [Range(0, 1)]
    [SerializeField] private float speechBubbleResizeTransitionTime = 0.2f;


    private Vector2 tailSize;
    private Vector2 tailSizeClosed;
    private Vector2 speechBubbleSizeClosed;

    private Vector2Int padding = new Vector2Int(50, 30);
    private TextEffect textEffect;

    #region Methods
    #region Initialisation
    protected void InitialiseCharacterCanvas()
    {
        textEffect = dialogueText.GetComponent<TextEffect>();

        tailSize = tail.sizeDelta;
        tailSizeClosed = new Vector2(0, tailSize.y);
        speechBubbleSizeClosed = new Vector2(tailSize.x, 0);

        continueIndicator.alpha = 0;
        tail.sizeDelta = tailSizeClosed;
        speechBubble.sizeDelta = speechBubbleSizeClosed;
    }

    private void Start()
    {
        DialogueController.Instance.OnConversationEnd += CloseTransition;
        ClearText();
    }
    #endregion

    private void FixedUpdate()
    {
        Vector2 containerPosition = dialogueContainer.anchoredPosition;
        containerPosition.y = Mathf.Sin(Time.fixedTime * Mathf.PI * speachBubbleFloatFrequency) * speachBubbleFloatAmplitude;
        dialogueContainer.anchoredPosition = containerPosition;
    }

    public virtual float OpenCloseTransitionTime()
    {
        return tailTransitionTime + speechBubbleOpenCloseTransitionTime;
    }

    public virtual float ResizeTransitionTime()
    {
        return speechBubbleResizeTransitionTime;
    }

    public virtual void OpenTransition(string _text)
    {
        if (IsOpen) ResizieTransition(_text);

        IsOpen = true;

        CancelLeanTween();
        ClearText();
        dialogueText.enableAutoSizing = false;

        LeanTween.size(tail, tailSize, tailTransitionTime);

        LeanTween.delayedCall(tailTransitionTime, callback =>
        {
            LeanTween.size(speechBubble, GetTargetSize(_text), speechBubbleOpenCloseTransitionTime);
        });
    }

    public virtual void CloseTransition()
    {
        if (!IsOpen) return;

        IsOpen = false;
        CancelLeanTween();

        continueIndicator.alpha = 0;
        dialogueText.enableAutoSizing = true;
        LeanTween.size(speechBubble, speechBubbleSizeClosed, speechBubbleOpenCloseTransitionTime);
        LeanTween.delayedCall(speechBubbleOpenCloseTransitionTime, callback =>
        {
            LeanTween.size(tail, tailSizeClosed, tailTransitionTime);
        });
    }

    public void ResizieTransition(string _text)
    {
        CancelLeanTween();

        continueIndicator.alpha = 0;
        LeanTween.size(speechBubble, GetTargetSize(_text), speechBubbleResizeTransitionTime);
    }

    public void SetText(string _text, bool _updateText  = true)
    {
        dialogueText.text = _text;
        if(_updateText) textEffect.UpdateText();
    }

    public void ClearText()
    {
        SetText("", false);
        textEffect.ClearText();
    }

    public void ShowContinueIndicator()
    {
        continueIndicator.alpha = 1;
    }

    #region Utility
    private Vector2 GetTargetSize(string _text)
    {
        // Save current text and speechBubble size
        Vector2 currentSize = speechBubble.sizeDelta;
        string currentText = dialogueText.text;

        // Set speech bubble size to max and text equal to target text
        speechBubble.sizeDelta = new Vector2(widthRange.y, heightRange.y);
        dialogueText.text = _text;

        // Set target size equal to text size + padding
        Vector2 targetSize = dialogueText.GetPreferredValues() + padding;
        targetSize.x = Mathf.Clamp(targetSize.x, widthRange.x, widthRange.y);
        targetSize.y = Mathf.Clamp(targetSize.y, heightRange.x, heightRange.y);

        // Set string back
        dialogueText.text = currentText;
        speechBubble.sizeDelta = currentSize;

        return targetSize;
    }

    protected virtual void CancelLeanTween()
    {
        LeanTween.cancel(speechBubble);
        LeanTween.cancel(tail);
    }
    #endregion
    #endregion
}