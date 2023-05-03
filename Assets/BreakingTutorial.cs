using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakingTutorial : MonoBehaviour
{
    [SerializeField] private RectTransform cursor;
    [SerializeField] private RectTransform button;

    [SerializeField] private float pressDuration = 0.5f;
    [SerializeField] private float releaseDuration = 0.125f;

    [SerializeField] private LeanTweenType pressEase = LeanTweenType.easeOutBack;
    [SerializeField] private LeanTweenType releaseEase = LeanTweenType.easeOutSine;

    private Vector3 scalePressed = new Vector3(1.25f, 1.25f, 1.25f);

    public void CancelCursor()
    {
        LeanTween.cancel(cursor);
        cursor.localScale = Vector3.one;
    }
    public void ClickDown()
    {
        LeanTween.scale(cursor.gameObject, scalePressed, pressDuration).setEase(pressEase);
        LeanTween.scale(button.gameObject, scalePressed, pressDuration).setEase(pressEase);
    }
    public void ClickUp()
    {
        LeanTween.scale(cursor.gameObject, Vector3.one, releaseDuration).setEase(releaseEase);
        LeanTween.scale(button.gameObject, Vector3.one, releaseDuration).setEase(releaseEase);
    }
}
