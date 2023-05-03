using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JumpTutorial : MonoBehaviour
{
    [Header("Manipulatable transforms")]
    [SerializeField] private RectTransform player;
    [SerializeField] private RectTransform jumpPrompt;

    [Header("Duration settings")]
    [SerializeField] private float duration = 2f;
    [SerializeField] private float moveByY = 350f;

    [Header("LeanTween settings")]
    [SerializeField] private LeanTweenType playerMoveEase = LeanTweenType.easeOutSine;
    [SerializeField] private LeanTweenType playerFallEase = LeanTweenType.easeInOutSine;
    [SerializeField] private LeanTweenType buttonPressEase = LeanTweenType.easeOutBack;
    [SerializeField] private LeanTweenType buttonReleaseEase = LeanTweenType.easeOutSine;

    private Vector3 shrinkVector = new Vector3(1.25f, 1.25f, 1f);

    private Vector2 playerStart;
    private float playerEnd;

    #region Initialisation
    //On awake, grab all current positions of objects in canvas space.
    //We will modify positions based on these values so the only thing that needs changing is start positions in the prefab itself.
    private void Awake()
    {
        playerStart = player.anchoredPosition;
        playerEnd = playerStart.y + moveByY;

    }

    //When this object is enabled, start tweening
    private void OnEnable()
    {
        Move1(); //Begin LeanTween sequence.
    }

    //When this object is disabled, stop tweening and reset.
    private void OnDisable()
    {
        LeanTween.cancel(player);
        LeanTween.cancel(jumpPrompt);
        player.anchoredPosition = playerStart;
        jumpPrompt.localScale = Vector3.one;
    }
    #endregion

    #region Sequence
    private void Move1()
    {
        //Scale button prompt
        LeanTween.cancel(player);
        LeanTween.scale(jumpPrompt.gameObject, shrinkVector, duration / 4).setEase(buttonPressEase).setDelay(duration / 4); //Wait for button to tween and then pop it up again (tapping button)
        LeanTween.scale(jumpPrompt.gameObject, Vector3.one, duration / 12).setEase(buttonReleaseEase);

        //Move player up
        LeanTween.moveY(player, playerEnd, duration / 2).setEase(playerMoveEase).setOnComplete(Move2);
    }

    private void Move2()
    {
        //Move player down halfway
        LeanTween.moveY(player, playerEnd/1.5f, duration/4).setEase(playerFallEase).setOnComplete(Move3);
    }

    private void Move3()
    {
        //Double jump
        LeanTween.scale(jumpPrompt.gameObject, shrinkVector, duration / 4).setEase(buttonPressEase).setDelay(duration / 4); //Wait for button to tween and then pop it up again (tapping button)
        LeanTween.scale(jumpPrompt.gameObject, Vector3.one, duration / 12).setEase(buttonReleaseEase);

        LeanTween.moveY(player, playerEnd+50, duration/2).setEase(playerMoveEase).setOnComplete(Move4);
    }
    private void Move4()
    {
        //Move player down to reset
        LeanTween.moveY(player, playerStart.y, duration / 2).setEase(playerFallEase).setOnComplete(Move1);
    }
    #endregion
}
