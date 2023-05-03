using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroCutsceneController : MonoBehaviour
{
    private enum ImageNames{ Forest01, Forest02, Forest03, Tree, CocoonClosed, CocoonLight, CocoonOpen, white }

    [SerializeField]private CanvasGroup[] cutSceneImages = new CanvasGroup[8];

    private void Awake()
    {
        foreach(CanvasGroup canvasGroup in cutSceneImages)
        {
            canvasGroup.alpha = 0;
        }

        StartCoroutine(CutScene());
    }

    private IEnumerator CutScene()
    {
        yield return new WaitForSeconds(1.5f);

        LeanTween.alphaCanvas(cutSceneImages[(int)ImageNames.Forest01], 1, 0.75f);
        yield return new WaitForSeconds(1.5f);

        LeanTween.alphaCanvas(cutSceneImages[(int)ImageNames.Forest02], 1, 0.75f);
        yield return new WaitForSeconds(1.5f);

        LeanTween.alphaCanvas(cutSceneImages[(int)ImageNames.Forest03], 1, 0.75f);
        yield return new WaitForSeconds(1.5f);

        LeanTween.alphaCanvas(cutSceneImages[(int)ImageNames.Tree], 1, 0.75f);
        yield return new WaitForSeconds(1.0f);

        LeanTween.size(cutSceneImages[(int)ImageNames.Tree].GetComponent<RectTransform>(), new Vector2(1.25f, 1.25f), 0.75f);
        yield return new WaitForSeconds(1.0f);

        LeanTween.alphaCanvas(cutSceneImages[(int)ImageNames.CocoonClosed], 1, 0.75f);
        yield return new WaitForSeconds(1.5f);

        LeanTween.alphaCanvas(cutSceneImages[(int)ImageNames.CocoonLight], 1, 0.75f);
        yield return new WaitForSeconds(1.5f);

        LeanTween.alphaCanvas(cutSceneImages[(int)ImageNames.CocoonOpen], 1, 0.75f);
        yield return new WaitForSeconds(3.0f);

        LeanTween.alphaCanvas(cutSceneImages[(int)ImageNames.white], 1, 0.75f);
        yield return new WaitForSeconds(3.0f);

        LoadingController.Instance.StartLoad();
    }
}