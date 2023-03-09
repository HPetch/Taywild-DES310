using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButtons : MonoBehaviour
{

    #region Variables
    [SerializeField] private int playSceneIndex = 1;
    #endregion


    #region Methods
    public void DoPlay()
    {
        SceneManager.LoadScene(playSceneIndex);
    }
    #endregion
}
