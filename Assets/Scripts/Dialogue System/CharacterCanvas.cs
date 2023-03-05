using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MinMaxSlider;

public class CharacterCanvas : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private RectTransform speechBubble;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private CanvasGroup dialogueCanvasGroup;
    private TextEffect textEffect;    

    [field: Range(0, 1)]
    [field: SerializeField] public float AlphaTransitionTime { get; private set; } = 0.2f;

    [field: Range(0, 1)]
    [field: SerializeField] public float SizeTransitionTime { get; private set; } = 0.4f;

    [MinMaxSlider(10,1000)]
    [SerializeField] private Vector2Int widthRange = new Vector2Int(100,600);

    [MinMaxSlider(10, 500)]
    [SerializeField] private Vector2Int heightRange = new Vector2Int(60,300);

    [SerializeField] private Vector2Int padding = new Vector2Int(50, 10);

    #region Methods
    #region Initialisation
    protected void InitialiseCharacterCanvas()
    {
        textEffect = dialogueText.GetComponent<TextEffect>();

        dialogueCanvasGroup.alpha = 0;
        speechBubble.sizeDelta = Vector2.zero;
    }

    private void Start()
    {
        DialogueController.Instance.OnConversationEnd += Hide;
    }
    #endregion

    public void Show(string _text)
    {
        LeanTween.cancel(dialogueCanvasGroup.gameObject);
        LeanTween.alphaCanvas(dialogueCanvasGroup, 1, AlphaTransitionTime);

        LeanTween.cancel(speechBubble);
        LeanTween.size(speechBubble, GetTargetSize(_text), SizeTransitionTime);
    }

    public void Hide()
    {
        ClearText();

        LeanTween.cancel(dialogueCanvasGroup.gameObject);
        LeanTween.alphaCanvas(dialogueCanvasGroup, 0, AlphaTransitionTime);

        LeanTween.cancel(speechBubble);
        LeanTween.size(speechBubble, new Vector2(speechBubble.sizeDelta.x, 0f), SizeTransitionTime);
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