using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingController : MonoBehaviour
{
    public static LoadingController Instance { get; private set; }

    [SerializeField] private CanvasGroup loadingScreen = null;

    [SerializeField] private Slider loadingSlider = null;
    [SerializeField] private RectTransform jasper = null;

    private bool isLoading = false;
    private AsyncOperation loadSceneAsyncOperation = null;

    private void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        loadingScreen.alpha = 0;
        loadingScreen.blocksRaycasts = false;
    }

    private void Update()
    {
        if (!isLoading) return;
        if (loadingScreen.alpha < 1.0f) return;

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

        loadSceneAsyncOperation.allowSceneActivation = true;
    }

    public void StartLoad()
    {
        if (isLoading) return;

        isLoading = true;
        loadingScreen.blocksRaycasts = true;
        LeanTween.alphaCanvas(loadingScreen, 1, 0.25f);

        loadSceneAsyncOperation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        loadSceneAsyncOperation.allowSceneActivation = false;
    }

    private void MoveSlider(float _value)
    {
        loadingSlider.value = Mathf.Lerp(loadingSlider.value, _value, 10 * Time.deltaTime);
        jasper.anchoredPosition = new Vector2(Mathf.Lerp(-270, 245, loadingSlider.value), -294);
    }
}