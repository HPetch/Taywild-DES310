using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateController : MonoBehaviour
{
    public static GameStateController Instance { get; private set; }

    public enum GameStates { PLAY, DIALOGUE, PAUSED }
    public GameStates GameState { get; private set; } = GameStates.PLAY;
    private bool wasDialoguePaused = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // Pauses or unpauses the game when escape is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (GameState)
            {
                case GameStates.PLAY:
                    Pause();
                    break;

                case GameStates.DIALOGUE:
                    wasDialoguePaused = true;
                    Pause();
                    break;

                case GameStates.PAUSED:
                    UnPause();
                    break;

            }
        }
    }

    // Pauses the player and enemies, only displays the pause menu if the player manually paused
    public void Pause()
    {
        GameState = GameStates.PAUSED;

        // Reveal Pause menu here
    }

    public void UnPause()
    {
        GameState = wasDialoguePaused ? GameStates.DIALOGUE : GameStates.PLAY;

        // Hide Pause menu here
    }

    public void DialoguePause()
    {
        if (GameState == GameStates.PLAY) GameState = GameStates.DIALOGUE;
    }

    public void DialogueUnPause()
    {
        wasDialoguePaused = false;
        if (GameState == GameStates.DIALOGUE) GameState = GameStates.PLAY;
    }
}
