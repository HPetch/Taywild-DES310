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
    [SerializeField] private RectTransform speechBubble;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private CanvasGroup tailCanvasGroup;
    private TextEffect textEffect;    

    [field: Range(0, 1)]
    [field: SerializeField] public float OpenCloseTransitionTime { get; private set; } = 0.4f;

    [field: Range(0, 1)]
    [field: SerializeField] public float ResizeTransitionTime { get; private set; } = 0.2f;

    [MinMaxSlider(10,1000)]
    [SerializeField] private Vector2Int widthRange = new Vector2Int(100,600);

    [MinMaxSlider(10, 500)]
    [SerializeField] private Vector2Int heightRange = new Vector2Int(60,300);

     private Vector2Int padding = new Vector2Int(50, 10);

    [SerializeField] private float speechBubbleStartingYPosition = -100;

    #region Methods
    #region Initialisation
    protected void InitialiseCharacterCanvas()
    {
        textEffect = dialogueText.GetComponent<TextEffect>();

        tailCanvasGroup.alpha = 0;
        speechBubble.sizeDelta = Vector2.zero;
        speechBubble.anchoredPosition = new Vector2(0, speechBubbleStartingYPosition);
        
        ClearText();
    }

    private void Start()
    {
        DialogueController.Instance.OnConversationEnd += Hide;
    }
    #endregion

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
        LeanTween.cancel(speechBubble.gameObject);
        LeanTween.cancel(tailCanvasGroup.gameObject);

        ClearText();
        dialogueText.enableAutoSizing = false;

        LeanTween.size(speechBubble, GetTargetSize(_text), OpenCloseTransitionTime / 2);
        LeanTween.moveY(speechBubble, 0, OpenCloseTransitionTime);
        LeanTween.delayedCall(OpenCloseTransitionTime / 2, callback => { tailCanvasGroup.alpha = 1; });
    }

    private void CloseTransition()
    {
        CharacterCanvasState = CharacterCanvasStates.CLOSED;

        LeanTween.cancel(speechBubble);
        LeanTween.cancel(speechBubble.gameObject);
        LeanTween.cancel(tailCanvasGroup.gameObject);

        dialogueText.enableAutoSizing = true;

        LeanTween.delayedCall(OpenCloseTransitionTime / 2, callback =>
        {
            LeanTween.size(speechBubble, Vector2.zero, OpenCloseTransitionTime / 2);
            tailCanvasGroup.alpha = 0;
        });

        LeanTween.moveY(speechBubble, speechBubbleStartingYPosition, OpenCloseTransitionTime);
    }

    private void ResizieTransition(string _text)
    {
        LeanTween.cancel(speechBubble);
        LeanTween.cancel(speechBubble.gameObject);
        LeanTween.cancel(tailCanvasGroup.gameObject);

        LeanTween.size(speechBubble, GetTargetSize(_text), ResizeTransitionTime);
        LeanTween.moveY(speechBubble, 0, OpenCloseTransitionTime);
        tailCanvasGroup.alpha = 1;
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