#if UNITY_EDITOR
using UnityEngine;

[ExecuteInEditMode]
public class PartitionDebugEditor : MonoBehaviour
{
    private Vector2 partitionSizeInUnits = new Vector2(35.5f, 20f);

    private Partition partition;

    // While in the editor change the size of the partitionSizeIndicator to the size of the partition
    private void Update()
    {
        partition = GetComponentInParent<Partition>();
        gameObject.SetActive(true);
        transform.localScale = new Vector3(partitionSizeInUnits.x * partition.PartitionSize.x, partitionSizeInUnits.y * partition.PartitionSize.y, 1);
        Random.InitState(Mathf.RoundToInt(transform.position.x + transform.position.y));
        GetComponent<SpriteRenderer>().color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.6f);
    }
}
#endif