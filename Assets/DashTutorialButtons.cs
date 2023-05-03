using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is a complex animation done through an Animator, so just have triggerable buttons here at Animation Events

public class DashTutorialButtons : MonoBehaviour
{
    [SerializeField] private RectTransform jumpButton;
    [SerializeField] private RectTransform cancelGlideButton;
    [SerializeField] private RectTransform dashButton;

    [SerializeField] private float pressDuration = 0.5f;
    [SerializeField] private float releaseDuration = 0.125f;

    [SerializeField] private LeanTweenType pressEase = LeanTweenType.easeOutBack;
    [SerializeField] private LeanTweenType releaseEase = LeanTweenType.easeOutSine;
    
    private Vector3 scalePressed = new Vector3(1.25f,1.25f,1.25f);

    public void CancelAnims()
    {
        LeanTween.cancel(jumpButton);
        LeanTween.cancel(cancelGlideButton);
        LeanTween.cancel(dashButton);
        jumpButton.localScale = Vector3.one;
        cancelGlideButton.localScale = Vector3.one;
        dashButton.localScale = Vector3.one;
    }

    public void JumpButton()
    {
        LeanTween.scale(jumpButton.gameObject, scalePressed, pressDuration).setEase(pressEase);
        LeanTween.scale(jumpButton.gameObject, Vector3.one, releaseDuration).setEase(releaseEase).setDelay(pressDuration); //Trigger this after press animation completes
    }

    public void DashButton()
    {
        LeanTween.scale(dashButton.gameObject, scalePressed, pressDuration).setEase(pressEase);
        LeanTween.scale(dashButton.gameObject, Vector3.one, releaseDuration).setEase(releaseEase).setDelay(pressDuration); //Trigger this after press animation completes
    }

    public void CancelGlideButton()
    {
        LeanTween.scale(cancelGlideButton.gameObject, scalePressed, pressDuration).setEase(pressEase);
        LeanTween.scale(cancelGlideButton.gameObject, Vector3.one, releaseDuration).setEase(releaseEase).setDelay(pressDuration); //Trigger this after press animation completes

    }
}
