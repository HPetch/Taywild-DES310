using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using UnityEngine;

public class DecorationMovingFake : MonoBehaviour
{
    [SerializeField] private GameObject[] CollisionCheckerArray;
    public enum ObjectType { DECORATION_OBJECT, ENVIROMENT_OBJECT, TRASH_OBJECT } //Decoration object - Move Edit Delete, Enviroment - Edit, Trash - Delete
    public ObjectType objectType;
    private bool isBeingMoved;
    private Vector3 moveTargetPosition;
    private Quaternion moveTargetRotation;
    [SerializeField] private float moveSpeed;
    private GameObject currentMoveTarget;
    [SerializeField] private ContactFilter2D placeableCheckContactFilter;
    [SerializeField] private LayerMask collisionCheckLayerMask;

    public void Inititalise(GameObject _currentMoveTarget)
    {
        currentMoveTarget = _currentMoveTarget;
        GetComponent<SpriteRenderer>().sprite = _currentMoveTarget.GetComponent<SpriteRenderer>().sprite;
        transform.position = _currentMoveTarget.transform.position;
        transform.rotation = _currentMoveTarget.transform.rotation;
        transform.localScale = _currentMoveTarget.transform.localScale;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, moveTargetPosition) > 0.01) //If the object is currently being moved, or hasn't reached their final placement after being placed
        { 
            transform.position = Vector3.Lerp(transform.position, DecorationController.Instance.DecorationSelector.transform.position, moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = moveTargetPosition;
            if (!isBeingMoved)
            {
                
            }
        }
            // Lerp current rotation to target rotation. Target rotation will 
        
    }

    /*public bool CheckIfPlaceable()
    {
        
        if(Physics2D.OverlapBoxAll()
        if(Physics2D.OverlapCircle(_attachPoint, currentMoveTarget.GetComponent<DecorationObject>().AttachmentPointRadius, ))
    }*/

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