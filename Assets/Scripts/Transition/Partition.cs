using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Partition : MonoBehaviour
{
    [field: SerializeField] public float TargetCameraSize { get; private set; } = 10f;
    [field: SerializeField] public bool IsCameraFixed { get; private set; } = true;
    [field: SerializeField] public Vector2Int PartitionSize { get; private set; } = Vector2Int.one;


    private void Awake()
    {
        GetComponentInChildren<PartitionDebugEditor>().gameObject.SetActive(false);
    }

    private void Update()
    {

    }
}