using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance { get; private set; }

    #region Methods
    #region Initialisation
    private void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }
    #endregion


    #region UI Buttons
    public void PlayButton()
    {

    }

    public void LoadButton()
    {

    }

    public void SettingsButton()
    {

    }

    public void CreditsButton()
    {

    }

    public void ExitButton()
    {
        Application.Quit();
    }
    #endregion
    #endregion
}