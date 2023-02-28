// Partition
// Game world consists of many partitions, each partition has some settings to dictate how the camera navigates the partition

using UnityEngine;

public class Partition : MonoBehaviour
{
    // The size in units of a chunk (view of the camera when ortho size is set to 10)
    public static Vector2 ChunkSizeInUnits { get; private set; } = new Vector2(35.5f, 20f);

    [field: Header("Partition")]
    // Partition size in chunks
    [field: SerializeField] public Vector2Int PartitionSize { get; private set; } = Vector2Int.one;

    [field: Header("Camera Settings")]
    [field: Range(5, 16)]
    [field: SerializeField] public float TargetCameraSize { get; private set; } = 10f;

    // The world size of the partition
    public Vector2 PartitionSizeInUnits { get { return ChunkSizeInUnits * PartitionSize; } }

    // A Rect representing the partition in world space, used to check if the player is within the partition
    public Rect PartitionRect { get { return new Rect((Vector2)transform.position - (PartitionSizeInUnits * 0.5f), PartitionSizeInUnits); } }
}