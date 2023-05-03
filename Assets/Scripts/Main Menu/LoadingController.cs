using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour
{
    public static LoadingController Instance { get; private set; }

    private CanvasGroup loadingCanvas = null;
    [SerializeField] private CanvasGroup loadingVisualCanvasGroup = null;

    [SerializeField] private Slider loadingSlider = null;
    [SerializeField] private RectTransform jasper = null;

    private bool isAsyncLoading = false;
    private AsyncOperation loadSceneAsyncOperation = null;

    [SerializeField] private string targetScene;

    private void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        loadingCanvas = GetComponent<CanvasGroup>();
        loadingCanvas.alpha = 0;
        loadingCanvas.blocksRaycasts = false;

        loadingVisualCanvasGroup.alpha = 1;

        loadingSlider.value = 0f;
        jasper.anchoredPosition = new Vector2(Mathf.Lerp(-270, 245, loadingSlider.value), -294);
    }

    private void Update()
    {
        if (!isAsyncLoading) return;
        if (loadingCanvas.alpha < 1.0f) return;

        if (loadSceneAsyncOperation.progress < 0.9f)
        {
            MoveSlider(loadSceneAsyncOperation.progress);
            return;
        }

        if (loadingSlider.value < 0.99f)
        {
            MoveSlider(1);
            return;
        }

        isAsyncLoading = false;
        StartCoroutine(SceneLoaded());
    }

    public void StartLoad()
    {
        if (isAsyncLoading) return;

        isAsyncLoading = true;
        loadingCanvas.blocksRaycasts = true;
        LeanTween.alphaCanvas(loadingCanvas, 1, 0.25f);

#if UNITY_EDITOR
        SceneManager.LoadScene(targetScene);
#else
        loadSceneAsyncOperation = SceneManager.LoadSceneAsync(targetScene);
        loadSceneAsyncOperation.allowSceneActivation = false;
#endif
    }

    private void MoveSlider(float _value)
    {
        loadingSlider.value = Mathf.Lerp(loadingSlider.value, _value, 2f * Time.deltaTime);
        jasper.anchoredPosition = new Vector2(Mathf.Lerp(-270, 245, loadingSlider.value), -294);
    }

    private IEnumerator SceneLoaded()
    {
        LeanTween.alphaCanvas(loadingVisualCanvasGroup, 0, 0.2f);
        yield return new WaitForSeconds(0.2f);
        loadSceneAsyncOperation.allowSceneActivation = true;
    }
}