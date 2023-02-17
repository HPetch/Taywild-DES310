// Transition Controller
// Controls the events triggering partition transition
// Partition transition is triggered when player position leaves the rect of the current partition

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TransitionController : MonoBehaviour
{
    public static TransitionController Instance { get; private set; } = null;

    #region Events
    public event Action<Partition> OnTransitionStart;
    public event Action OnTransitionEnd;
    #endregion

    #region Variables
    /// <summary>
    /// If enabled partition debug info is displayed  
    /// </summary>
    [field: SerializeField] public bool DisplayDebugInfo { get; private set; } = true;

    /// <summary>
    /// Reference to the current partition the player is  
    /// </summary>
    public Partition CurrentPartition { get; private set; } = null;
    public bool IsTransitioning { get; private set; } = false;

    [SerializeField] private Partition[] partitions;
    #endregion

    #region Functions
    #region Initialisation
    private void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }

    private void Start()
    {
        // Defaults the current partition to whatever partition the play is starting in
        CurrentPartition = FindPlayerPartiton();
    }
    #endregion

    private void Update()
    {
        // If the player is not inside the current partition
        if(!CurrentPartition.PartitionRect.Contains(PlayerController.Instance.transform.position))
        {
            // Find the partition that the player is in
            TriggerTransition(FindPlayerPartiton());
        }
    }

    /// <summary>
    /// Finds the partition that the player is in
    /// </summary>
    private Partition FindPlayerPartiton()
    {
        // Foreach partition
        foreach(Partition partition in partitions)
        {
            // If the player is within the partition rect
            if (partition.PartitionRect.Contains(PlayerController.Instance.transform.position))
            {
                // This is the current partition
                return partition;
            }
        }

        // If the player is not inside any partition then...
        // TO DO: Talk to Harry and decide how to handle this event
        Debug.LogError("Player partiton not found");
        return CurrentPartition;
    }

    /// <summary>
    /// Trigger a transition to the target partition
    /// </summary>
    private void TriggerTransition(Partition _targetPartition)
    {
        // Cannot trigger a transition while already transitioning
        if (IsTransitioning) return;

        IsTransitioning = true;
        CurrentPartition = _targetPartition;

        OnTransitionStart?.Invoke(CurrentPartition);
        StartCoroutine(Transition());
    }

    /// <summary>
    /// Coroutine that executes the transition effect
    /// </summary>
    private IEnumerator Transition()
    {
        yield return new WaitForSeconds(0.3f);
        IsTransitioning = false;
        OnTransitionEnd?.Invoke();
    }

    // Reset automatically references each partition in editor
    private void Reset()
    {
        partitions = GetComponentsInChildren<Partition>();
    }
    #endregion
}
