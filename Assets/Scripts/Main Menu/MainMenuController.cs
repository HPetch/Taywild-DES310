using Journal;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public enum MenuPages { Cover, MainMenu, Settings, Credits}

    public static MainMenuController Instance { get; private set; }

    #region Variables
    [SerializeField] private JournalController journal = null;
    #endregion

    #region Methods
    #region Initialisation
    private void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }
    #endregion

    private void Update()
    {
        if (Input.anyKeyDown && journal.currentPaper == (int)MenuPages.Cover)
        {
            journal.FlipToPage((int)MenuPages.MainMenu);
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            if (journal.currentPaper != (int)MenuPages.MainMenu)
            {
                journal.FlipToPage((int)MenuPages.MainMenu);
            }
        }
    }


    #region UI Buttons
    public void PlayButton()
    {
        LoadingController.Instance.StartLoad();
    }

    public void LoadButton()
    {

    }

    public void SettingsButton()
    {
        journal.FlipToPage((int)MenuPages.Settings);
    }

    public void CreditsButton()
    {
        journal.FlipToPage((int)MenuPages.Credits);
    }

    public void ExitButton()
    {
    #if UNITY_STANDALONE_WIN
        Application.Quit();
    #endif
    }
    #endregion
    #endregion
}