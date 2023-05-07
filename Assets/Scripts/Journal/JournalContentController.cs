using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Journal;

public class JournalContentController : MonoBehaviour
{
    public static JournalContentController Instance { get; private set; }

    private static readonly int coverPageNumber = 0;
    private static readonly int pauseMenuPageNumber = 1;
    private static readonly int charactersPageNumber = 2;
    private static readonly int mapPageNumber = 6;
    private static readonly int settingsPageNumber = 7;
    private static readonly int creditsPageNumber = 8;

    #region Variables
    public bool IsOpen { get; private set; } = false;

    [SerializeField] private JournalController journal = null;
    [SerializeField] private CanvasGroup journalCanvasGroup = null;

    private int queuedChapterInput = pauseMenuPageNumber;
    #endregion

    #region Methods
    #region Initialisation
    void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        journalCanvasGroup.interactable = false;
    }

    #endregion

    private void Update()
    {
        if (!IsOpen)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OpenJournal();
            }
            else if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.J))
            {
                queuedChapterInput = charactersPageNumber;
                OpenJournal();
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                queuedChapterInput = mapPageNumber;
                OpenJournal();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If on the pause menu, close the journal
            if (journal.CurrentPaper == pauseMenuPageNumber)
            {
                CloseJournal();
                return;
            }

            // Else go to the pause menu
            journal.FlipToPage(pauseMenuPageNumber);
            return;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (journal.CurrentPaper != charactersPageNumber)
            {
                journal.FlipToPage(charactersPageNumber);
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            if (journal.CurrentPaper != mapPageNumber)
            {
                journal.FlipToPage(mapPageNumber);
            }

            return;
        }
    }

    private void OpenJournal()
    {
        IsOpen = true;
        journalCanvasGroup.interactable = true;

        if (queuedChapterInput != pauseMenuPageNumber)
        {
            journal.FlipToPage(queuedChapterInput);
            queuedChapterInput = pauseMenuPageNumber;
            return;
        }

        journal.FlipToPage(pauseMenuPageNumber);
    }

    private void CloseJournal()
    {
        IsOpen = false;
        journalCanvasGroup.interactable = false;
        journal.FlipToPage(coverPageNumber);
    }

    #region UI Buttons
    #region Pause Menu
    public void ResumeButton()
    {
        CloseJournal();
    }

    public void SettingsButton()
    {
        JournalController.Instance.FlipToPage(settingsPageNumber);
    }

    public void MenuButton()
    {
        LoadingController.Instance.StartLoad();
    }

    public void ExitButton()
    {
        // WEBGL doesn't support Application.Quit
        MenuButton();        
    }
    #endregion

    #region Tabs
    public void CharactersTabButton()
    {
        if (journal.CurrentPaper != charactersPageNumber)
        {
            journal.FlipToPage(charactersPageNumber);
        }
    }

    public void MapTabButton()
    {
        if (journal.CurrentPaper != mapPageNumber)
        {
            journal.FlipToPage(mapPageNumber);
        }
    }

    public void SettingsTabButton()
    {
        if (journal.CurrentPaper != settingsPageNumber)
        {
            journal.FlipToPage(settingsPageNumber);
        }
    }

    public void CreditsTabButton()
    {
        if (journal.CurrentPaper != creditsPageNumber)
        {
            journal.FlipToPage(creditsPageNumber);
        }
    }
    #endregion
    #endregion
    #endregion
}