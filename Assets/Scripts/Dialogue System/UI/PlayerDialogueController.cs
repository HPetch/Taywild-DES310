using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DialogueSystem.ScriptableObjects;

public class PlayerDialogueController : CharacterCanvas
{
    public static PlayerDialogueController Instance { get; private set; }

    #region Variables
    [Space(10)]
    [Header("Dialogue References")]
    [SerializeField] private ThoughtBubble leftThoughtBubble;
    [SerializeField] private ThoughtBubble middleThoughtBubble;
    [SerializeField] private ThoughtBubble rightThoughtBubble;

    [Range(0, 0.5f)]
    [SerializeField] private float delayBetweenThoughBubbleTransitions = 0.25f;

    public bool ThoughtBubbleTransitionComplete { get; private set; } = true;
    #endregion

    #region Methods
    #region Initialisation
    private void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        InitialiseCharacterCanvas();
    }
    #endregion

    public void ShowThoughtBubbles(DialogueSystemDialogueSO _dialogueNode)
    {
        ThoughtBubbleTransitionComplete = false;

        switch (_dialogueNode.Choices.Count)
        {
            case 0:
                Debug.LogError("ShowThoughtBubbles function called but _dialogueNode had no choices");
                break;

            case 1:
                middleThoughtBubble.TransitionIn(_dialogueNode.Choices[0]);
                LeanTween.delayedCall(middleThoughtBubble.GetTransitionTime(), callback =>
                {
                    ThoughtBubbleTransitionComplete = true;
                });
                break;

            case 2:
                leftThoughtBubble.TransitionIn(_dialogueNode.Choices[0]);
                LeanTween.delayedCall(delayBetweenThoughBubbleTransitions, callback =>
                {
                    rightThoughtBubble.TransitionIn(_dialogueNode.Choices[1]);
                    LeanTween.delayedCall(rightThoughtBubble.GetTransitionTime(), callback =>
                    {
                        ThoughtBubbleTransitionComplete = true;
                    });
                });
                break;

            default:
                Debug.LogError("_dialogueNode has more than 3 choices");
                goto case 3;

            case 3:
                leftThoughtBubble.TransitionIn(_dialogueNode.Choices[0]);
                LeanTween.delayedCall(delayBetweenThoughBubbleTransitions, callback =>
                {
                    middleThoughtBubble.TransitionIn(_dialogueNode.Choices[1]);
                    LeanTween.delayedCall(delayBetweenThoughBubbleTransitions, callback =>
                    {
                        rightThoughtBubble.TransitionIn(_dialogueNode.Choices[2]);
                        LeanTween.delayedCall(rightThoughtBubble.GetTransitionTime(), callback =>
                        {
                            ThoughtBubbleTransitionComplete = true;
                        });
                    });
                });
                break;
        }
    }

    public void HideThoughtBubbles(int _chosenOption)
    {
        ThoughtBubbleTransitionComplete = false;

        if (leftThoughtBubble.IsOpen) leftThoughtBubble.TransitionOut();
        if (middleThoughtBubble.IsOpen) middleThoughtBubble.TransitionOut();
        if (rightThoughtBubble.IsOpen) rightThoughtBubble.TransitionOut();

        LeanTween.delayedCall(middleThoughtBubble.GetTransitionTime(), callback =>
        {
            ThoughtBubbleTransitionComplete = true;
        });
    }

    public void OnThoughtBubblePressed(int _chosenThought)
    {
        if (!ThoughtBubbleTransitionComplete) return;

        DialogueController.Instance.BranchButton(_chosenThought);
    }
    #endregion
}