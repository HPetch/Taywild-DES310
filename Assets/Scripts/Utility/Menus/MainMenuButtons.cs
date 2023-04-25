using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{

    #region Variables
    [SerializeField] private int playSceneIndex = 1;
    [SerializeField] private GameObject currentlyOpenCanvas;
    [SerializeField] private TMP_Text versionText;
    #endregion


    #region Methods

    private void Awake()
    {
        versionText.text = "v" + Application.version;
    }
    public void DoPlay()
    {
        //Insert fancier stuff like a fade to black here.
        SceneManager.LoadScene(playSceneIndex);
    }

    public void SwapCanvas(GameObject toCanvas)
    {
        //Disable old canvas, enable new canvas, replace current canvas reference.
        currentlyOpenCanvas.SetActive(false);
        toCanvas.SetActive(true);
        currentlyOpenCanvas = toCanvas;
    }
    #endregion
}
