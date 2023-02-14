using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using UnityEngine;

public class DecorationObject : MonoBehaviour
{
    [SerializeField] private GameObject[] CollisionCheckerArray;
    public enum ObjectType { DECORATION_OBJECT, ENVIROMENT_OBJECT, TRASH_OBJECT } //Decoration object - Move Edit Delete, Enviroment - Edit, Trash - Delete
    public ObjectType objectType;
    private bool isBeingMoved;
    private Vector3 moveTargetPosition;
    private Quaternion moveTargetRotation;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isBeingMoved || transform.position != moveTargetPosition) //If the object is currently being moved, or hasn't reached their final placement after being placed
        {
            transform.position = DecorationController.Instance.DecorationSelector.transform.position;
            // Lerp current rotation to target rotation. Target rotation will 
        }
    }

    public void StartSelectorFollow()
    {
        isBeingMoved = true;
    }
    public void EndSelectorFollow(Transform _placedTransform)
    {
        isBeingMoved = false;
        moveTargetPosition = _placedTransform.position;
        moveTargetRotation = _placedTransform.rotation;
    }
}