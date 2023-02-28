// Partition Debugger
// If display debug information is enabled on the TransitionController then
// PartitionDebugger will render a sprite representing the partition with a unique colour

using UnityEngine;

[ExecuteInEditMode]
public class PartitionDebuger : MonoBehaviour
{
    #if !UNITY_EDITOR
    private void Awake()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }
    #endif

    #if UNITY_EDITOR
    // While in the editor change the size of the partitionSizeIndicator to the size of the partition
    private void Update()
    {
        // Reference the partition
        Partition partition = GetComponentInParent<Partition>();

        // If the TransitionController has debug info enabled
        if (partition.GetComponentInParent<TransitionController>().DisplayDebugInfo)
        {
            // Enable the sprite renderer
            GetComponent<SpriteRenderer>().enabled = true;
            // Scale it to partiton size
            transform.localScale = partition.PartitionSizeInUnits;
            // Set random seed based on position so that the colour is consistant among updates
            Random.InitState(Mathf.RoundToInt(transform.position.x + transform.position.y));
            // Set colour
            GetComponent<SpriteRenderer>().color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.6f);
            // Set the seed back to random
            Random.InitState((int)System.DateTime.Now.Ticks);
        }
        else
        {
            // If debug info is disabled then hide the sprite renderer
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
    #endif
}