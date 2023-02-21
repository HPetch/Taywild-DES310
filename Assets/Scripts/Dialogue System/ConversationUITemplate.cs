using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ConversationUITemplate : MonoBehaviour
{
    [System.Serializable]
    public struct BranchButton
    {
        public CanvasGroup canvasGroup;
        public TextMeshProUGUI buttonText;
        public Image buttonBckground;
    };

    public enum ConversationUITemplates {  BOTTOM_LEFT, BOTTOM_MIDDLE_LONG, BOTTOM_MIDDLE_SHORT, TOP_LEFT};

    #region Variables
    [field: Header("Template Type")]
    [field: SerializeField] public ConversationUITemplates UITemplate { get; private set; }

    [field: Header("Portrait")]
    [field: SerializeField] public Image CharacterPortrait { get; private set; } = null;
    [SerializeField] protected Image characterPortraitBorder = null;

    [field: Header("Name")]
    [field: SerializeField] public TextMeshProUGUI CharacterName { get; private set; } = null;
    [SerializeField] protected Image characterNameBackground = null;

    [field: Header("Text")]
    [field: SerializeField] public TextMeshProUGUI TextField { get; private set; } = null;
    [SerializeField] protected Image textFieldBackground = null;

    [field: Header("Buttons")]
    [field: SerializeField] public BranchButton[] BranchButtons { get; private set; } = new BranchButton[4];

    private CanvasGroup canvasGroup;
    #endregion

    #region Functions
    #region Initialisation
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }
    #endregion

    #region
    private IEnumerator TransitionIn()
    {
        yield return null;
    }

    private IEnumerator TransitionOut()
    {
        yield return null;
    }

    private IEnumerator ChangeCharacter()
    {
        yield return null;
    }
    #endregion

    #region Utility
    private void Reset()
    {
        Transform character = transform.Find("Character");

        characterPortraitBorder = character.Find("Character Portrait Background").GetComponent<Image>();
        CharacterPortrait = characterPortraitBorder.transform.Find("Character Portrait").GetComponent<Image>();

        characterNameBackground = character.Find("Character Name Background").GetComponent<Image>();
        CharacterName = characterNameBackground.transform.Find("Character Name").GetComponent<TextMeshProUGUI>();

        textFieldBackground = character.Find("Text Background").GetComponent<Image>();
        TextField = textFieldBackground.transform.Find("Dialgoue Text").GetComponent<TextMeshProUGUI>();

        Transform branchButtons = transform.Find("Branch Buttons");
        BranchButtons = new BranchButton[4];

        for (int button = 0; button < 4; button++)
        {
            BranchButtons[button].buttonBckground = branchButtons.Find("Button " + (button + 1)).GetComponent<Image>();
            BranchButtons[button].canvasGroup = BranchButtons[button].buttonBckground.GetComponent<CanvasGroup>();
            BranchButtons[button].buttonText = BranchButtons[button].buttonBckground.transform.Find("Button " + (button + 1) + " Text").GetComponent<TextMeshProUGUI>();
        }
    }
    #endregion
    #endregion
}