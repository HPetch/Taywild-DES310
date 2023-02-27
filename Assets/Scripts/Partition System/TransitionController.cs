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


    public Partition CurrentPartition { get; private set; } = null;
    public bool IsTransitioning { get; private set; } = false;

    [SerializeField] private Partition[] partitions;

    private PlayerController player;
    #endregion

    #region Functions
    #region Initialisation
    private void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        DisplayDebugInfo = false;
    }

    private void Start()
    {
        player = PlayerController.Instance;

        // Defaults the current partition to whatever partition the play is starting in
        TriggerTransition(FindPlayerPartiton());
    }
    #endregion

    private void Update()
    {
        // If the player is not inside the current partition
        if(!CurrentPartition.PartitionRect.Contains(player.transform.position))
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
            if (partition.PartitionRect.Contains(player.transform.position))
            {
                // This is the current partition
                return partition;
            }
        }

        // If the player is not inside any partition then reset them to the last know ground position
        Debug.LogWarning("Player partiton not found");
        player.ResetPlayerToLastKnownPosition();
        return CurrentPartition;
    }

    /// <summary>
    /// Trigger a transition to the target partition
    /// </summary>
    private void TriggerTransition(Partition targetPartition)
    {
        // If we attempt to transition to the current partition, return
        if (CurrentPartition == targetPartition) return;

        IsTransitioning = true;
        CurrentPartition = targetPartition;

        OnTransitionStart?.Invoke(CurrentPartition);
        StartCoroutine(Transition());
    }

    /// <summary>
    /// Coroutine that executes the transition effect
    /// </summary>
    private IEnumerator Transition()
    {
        yield return new WaitForSeconds(1f);
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
