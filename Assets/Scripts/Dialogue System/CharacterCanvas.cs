using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MinMaxSlider;

public class CharacterCanvas : MonoBehaviour
{
    public enum CharacterCanvasStates { CLOSED, OPEN }
    public CharacterCanvasStates CharacterCanvasState { get; private set; } = CharacterCanvasStates.CLOSED;


    [Header("Dialogue References")]
    [SerializeField] private RectTransform dialogueContainer;
    [SerializeField] private RectTransform speechBubble;
    [SerializeField] private RectTransform tail;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Space(10)]
    [Header("Speech Box Constraints")]
    [MinMaxSlider(10, 1000)]
    [SerializeField] private Vector2Int widthRange = new Vector2Int(100, 600);

    [MinMaxSlider(10, 500)]
    [SerializeField] private Vector2Int heightRange = new Vector2Int(60, 300);

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

        tail.sizeDelta = tailSizeClosed;
        speechBubble.sizeDelta = speechBubbleSizeClosed;
        
        ClearText();
    }

    private void Start()
    {
        DialogueController.Instance.OnConversationEnd += Hide;
    }
    #endregion

    public virtual float OpenCloseTransitionTime()
    {
        return tailTransitionTime + speechBubbleOpenCloseTransitionTime;
    }

    public virtual float ResizeTransitionTime()
    {
        return speechBubbleResizeTransitionTime;
    }

    public void Show(string _text)
    {
        if (CharacterCanvasState == CharacterCanvasStates.CLOSED)
        {
            OpenTransition(_text);
        }
        else
        {
            ResizieTransition(_text);
        }
    }

    public void Hide()
    {
        if (CharacterCanvasState == CharacterCanvasStates.OPEN) CloseTransition();
    }

    protected virtual void OpenTransition(string _text)
    {
        CharacterCanvasState = CharacterCanvasStates.OPEN;

        LeanTween.cancel(speechBubble);
        LeanTween.cancel(tail);

        ClearText();
        dialogueText.enableAutoSizing = false;

        LeanTween.size(tail, tailSize, tailTransitionTime);

        LeanTween.delayedCall(tailTransitionTime, callback =>
        {
            LeanTween.size(speechBubble, GetTargetSize(_text), speechBubbleOpenCloseTransitionTime);
        });
    }

    protected virtual void CloseTransition()
    {
        CharacterCanvasState = CharacterCanvasStates.CLOSED;

        LeanTween.cancel(speechBubble);
        LeanTween.cancel(tail);

        dialogueText.enableAutoSizing = true;
        LeanTween.size(speechBubble, speechBubbleSizeClosed, speechBubbleOpenCloseTransitionTime);
        LeanTween.delayedCall(speechBubbleOpenCloseTransitionTime, callback =>
        {
            LeanTween.size(tail, tailSizeClosed, tailTransitionTime);
        });
    }

    private void ResizieTransition(string _text)
    {
        LeanTween.cancel(speechBubble);
        LeanTween.cancel(tail);

        LeanTween.size(speechBubble, GetTargetSize(_text), speechBubbleResizeTransitionTime);
    }

    public void SetText(string _text)
    {
        dialogueText.text = _text;
        textEffect.UpdateText();
    }

    public void ClearText()
    {
        SetText("");
        textEffect.ClearText();
    }

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
    #endregion
}