using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MinMaxSlider;

public class CharacterCanvas : MonoBehaviour
{
    public enum CharacterCanvasStates { CLOSED, OPEN }
    public CharacterCanvasStates CharacterCanvasState { get; private set; } = CharacterCanvasStates.CLOSED;


    [Header("Dialogue Settings")]
    [SerializeField] private RectTransform dialogueContainer;
    [SerializeField] private RectTransform speechBubble;
    [SerializeField] private RectTransform tail;
    [SerializeField] private TextMeshProUGUI dialogueText;


    [field: Range(0, 10)]
    [field: SerializeField] public float OpenCloseTransitionTime { get; private set; } = 0.4f;

    [field: Range(0, 10)]
    [field: SerializeField] public float ResizeTransitionTime { get; private set; } = 0.2f;

    [MinMaxSlider(10,1000)]
    [SerializeField] private Vector2Int widthRange = new Vector2Int(100,600);

    [MinMaxSlider(10, 500)]
    [SerializeField] private Vector2Int heightRange = new Vector2Int(60,300);

    private float speechBubbleStartingYPosition = -100;
    private Vector2 tailSize;
    private Vector2 tailSizeClosed;
    private Vector2 speechBubbleSizeClosed;

    private Vector2Int padding = new Vector2Int(50, 30);
    private CanvasGroup tailCanvasGroup;
    private TextEffect textEffect;

    #region Methods
    #region Initialisation
    protected void InitialiseCharacterCanvas()
    {
        textEffect = dialogueText.GetComponent<TextEffect>();
        tailCanvasGroup = tail.GetComponent<CanvasGroup>();

        tailSize = tail.sizeDelta;
        tailSizeClosed = new Vector2(0, tailSize.y);
        speechBubbleSizeClosed = new Vector2(tailSize.x, 0);

        //tailCanvasGroup.alpha = 0;
        tail.sizeDelta = tailSizeClosed;
        speechBubble.sizeDelta = speechBubbleSizeClosed;
        //speechBubble.anchoredPosition = new Vector2(0, speechBubbleStartingYPosition);
        
        ClearText();
    }

    private void Start()
    {
        DialogueController.Instance.OnConversationEnd += Hide;
    }
    #endregion

    private void Update()
    {
        
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

    private void OpenTransition(string _text)
    {
        CharacterCanvasState = CharacterCanvasStates.OPEN;

        LeanTween.cancel(speechBubble);
        LeanTween.cancel(tail);

        ClearText();
        dialogueText.enableAutoSizing = false;

        LeanTween.size(tail, tailSize, 0.1f);

        LeanTween.delayedCall(0.1f, callback =>
        {
            LeanTween.size(speechBubble, GetTargetSize(_text), 0.3f);
            //LeanTween.moveY(speechBubble, 0, OpenCloseTransitionTime);
        });
    }

    private void CloseTransition()
    {
        CharacterCanvasState = CharacterCanvasStates.CLOSED;

        LeanTween.cancel(speechBubble);
        LeanTween.cancel(tail);

        dialogueText.enableAutoSizing = true;
        LeanTween.size(speechBubble, speechBubbleSizeClosed, 0.3f);
        LeanTween.delayedCall(0.3f, callback =>
        {
            LeanTween.size(tail, tailSizeClosed, 0.1f);
        });

        //LeanTween.moveY(speechBubble, speechBubbleStartingYPosition, OpenCloseTransitionTime);
    }

    private void ResizieTransition(string _text)
    {
        LeanTween.cancel(speechBubble);
        LeanTween.cancel(tail);

        LeanTween.size(speechBubble, GetTargetSize(_text), ResizeTransitionTime);
        //LeanTween.moveY(speechBubble, 0, OpenCloseTransitionTime);
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