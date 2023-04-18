using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{

    #region Variables
    [SerializeField] private int playSceneIndex = 1;
    [SerializeField] private GameObject currentlyOpenCanvas;
    #endregion


    #region Methods
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
