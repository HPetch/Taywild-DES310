#if UNITY_EDITOR
using UnityEngine;

[ExecuteInEditMode]
public class DoorDebugEditor : MonoBehaviour
{
    void Update()
    {
        Door door = GetComponentInParent<Door>();

        gameObject.SetActive(true);
        transform.localScale = door.GetComponent<BoxCollider2D>().size;
        
        if (door.Partition == null)
        {
            Debug.LogWarning("Door 'Target Partition' not set");
        }
        else
        {
            GetComponent<SpriteRenderer>().color = door.Partition.GetComponentInChildren<PartitionDebugEditor>().GetComponent<SpriteRenderer>().color;
        }
    }
}
#endif