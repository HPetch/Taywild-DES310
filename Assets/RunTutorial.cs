using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RunTutorial : MonoBehaviour
{
    [Header("Manipulatable transforms")]
    [SerializeField] private RectTransform player;
    [SerializeField] private RectTransform leftPrompt;
    [SerializeField] private RectTransform rightPrompt;
    
    [Header("Duration settings")]
    [SerializeField] private float duration = 2f;
    [SerializeField] private float moveByX = 100f;

    [Header("LeanTween settings")]
    [SerializeField] private LeanTweenType playerMoveEase = LeanTweenType.easeInOutSine;
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
        playerEnd = playerStart.x + moveByX;

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
        LeanTween.cancel(leftPrompt);
        LeanTween.cancel(rightPrompt);
        player.anchoredPosition = playerStart;
        leftPrompt.localScale = Vector3.one;
        rightPrompt.localScale = Vector3.one;
    }
    #endregion

    #region Sequence
    private void Move1()
    {
        //Scale right button prompt
        LeanTween.cancel(player);
        LeanTween.scale(leftPrompt.gameObject, Vector3.one, duration / 12).setEase(buttonReleaseEase);
        LeanTween.scale(rightPrompt.gameObject, shrinkVector, duration / 4).setEase(buttonPressEase);

        //Move player right
        player.localScale = Vector3.one;
        LeanTween.moveX(player,playerEnd,duration).setEase(playerMoveEase).setOnComplete(Move2);
    }

    private void Move2()
    {
        //Scale left button prompt
        LeanTween.scale(leftPrompt.gameObject, shrinkVector, duration / 4).setEase(buttonPressEase);
        LeanTween.scale(rightPrompt.gameObject, Vector3.one, duration / 12).setEase(buttonReleaseEase);

        //Move player left
        player.localScale = new Vector3(-1f, 1f, 1f);
        LeanTween.moveX(player, playerStart.x, duration).setEase(playerMoveEase).setOnComplete(Move1);
    }
    #endregion
}
