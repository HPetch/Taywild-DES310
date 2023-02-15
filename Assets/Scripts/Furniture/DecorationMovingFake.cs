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
    [SerializeField] private ContactFilter2D placeableCheckContactFilter;
    [SerializeField] private LayerMask collisionCheckLayerMask;

    public void Inititalise()
    {
        GetComponent<SpriteRenderer>().sprite = DecorationController.Instance.CurrentMoveTarget.GetComponent<SpriteRenderer>().sprite;
        transform.position = DecorationController.Instance.CurrentMoveTarget.transform.position;
        transform.rotation = DecorationController.Instance.CurrentMoveTarget.transform.rotation;
        transform.localScale = DecorationController.Instance.CurrentMoveTarget.transform.localScale;
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
        
        if (CheckIfPlaceable())
        {
            GetComponent<SpriteRenderer>().color = Color.white;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
    }

    public bool CheckIfPlaceable() // Checks if any decorations are overlaping the held decoration's placement, and if all attach points are valid
    {
        bool placeable = false;
        if (Physics2D.OverlapBoxAll(new Vector2(transform.position.x, transform.position.y) + DecorationController.Instance.CurrentMoveTarget.GetComponent<BoxCollider2D>().offset,
 DecorationController.Instance.CurrentMoveTarget.GetComponent<BoxCollider2D>().size, 0.0f).Length == 0) // ADD ROTATION // Checks if any other decorations or a platform is overlapping the decoration
        {
            int successfulAttachPoints = 0;
            foreach (Vector3 _attachPoint in DecorationController.Instance.CurrentMoveTarget.GetComponent<DecorationObject>().AttachmentPointsList) // Goes through each attach point
            {
                
                if (Physics2D.OverlapCircle(_attachPoint, DecorationController.Instance.CurrentMoveTarget.GetComponent<DecorationObject>().AttachmentPointRadius, collisionCheckLayerMask))
                {
                    successfulAttachPoints++;
                }
                if (successfulAttachPoints == DecorationController.Instance.CurrentMoveTarget.GetComponent<DecorationObject>().AttachmentPointsList.Count) // Checks if all attach points are valid
                {
                    placeable = true;
                }
            }
        }
        return placeable;
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