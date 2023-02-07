using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionController : MonoBehaviour
{
    public static TransitionController Instance { get; private set; }

    #region Events
    #endregion

    #region Variables
    public bool DisplayDebugInfo { get; private set; } = true;

    public Partition currentPartition { get; private set; } = null;
    #endregion

    #region Functions
    #region Initialisation
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public void Transition(Partition _targetPartition)
    {
        
    }
    #endregion
}