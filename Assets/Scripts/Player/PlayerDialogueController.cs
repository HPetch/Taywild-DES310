using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueController : CharacterCanvas
{
    public static PlayerDialogueController Instance { get; private set; }

    #region Variables
    #endregion

    #region Methods
    #region Initialisation
    private void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        InitialiseCharacterCanvas();
    }
    #endregion

    protected void Update()
    {

    }
    #endregion
}