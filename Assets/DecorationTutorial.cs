using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationTutorial : MonoBehaviour
{
    [SerializeField] private RectTransform button;
    [SerializeField] private RectTransform cursor;

    [SerializeField] private float pressDuration = 0.5f;
    [SerializeField] private float releaseDuration = 0.125f;

    [SerializeField] private LeanTweenType pressEase = LeanTweenType.easeOutBack;
    [SerializeField] private LeanTweenType releaseEase = LeanTweenType.easeOutSine;

    private Vector3 scalePressed = new Vector3(1.25f, 1.25f, 1.25f);

    public void CancelAnims()
    {
        CancelCursor();
        CancelButton();
    }
    public void CancelCursor()
    {
        LeanTween.cancel(cursor);
        cursor.localScale = Vector3.one;
    }
    public void CancelButton()
    {
        LeanTween.cancel(button);
        button.localScale = Vector3.one;
    }

    public void PressButton()
    {
        LeanTween.scale(button.gameObject, scalePressed, pressDuration).setEase(pressEase);
        LeanTween.scale(button.gameObject, Vector3.one, releaseDuration).setEase(releaseEase).setDelay(pressDuration);
    }

    public void ClickMouse()
    {
        LeanTween.scale(cursor.gameObject, scalePressed, pressDuration).setEase(pressEase);
        LeanTween.scale(cursor.gameObject, Vector3.one, releaseDuration).setEase(releaseEase).setDelay(pressDuration);
    }
}
