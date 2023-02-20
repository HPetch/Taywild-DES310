using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using UnityEngine;

public class DecorationMovingFake : MonoBehaviour
{
    private GameObject moveTarget;
    [SerializeField] private GameObject[] CollisionCheckerArray;
    public enum ObjectType { DECORATION_OBJECT, ENVIROMENT_OBJECT, TRASH_OBJECT } //Decoration object - Move Edit Delete, Enviroment - Edit, Trash - Delete
    public ObjectType objectType { get; private set; }
    public bool isBeingMoved { get; private set; }
    private bool isPlaced;
    private Vector3 moveTargetPosition;
    private Quaternion moveTargetRotation;
    [SerializeField] private float moveSpeed;
    [SerializeField] private ContactFilter2D placeableCheckContactFilter;
    [SerializeField] private LayerMask collisionCheckLayerMask;

    public void Inititalise()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
        moveTarget = DecorationController.Instance.CurrentMoveTarget;
        GetComponent<SpriteRenderer>().sprite = moveTarget.GetComponent<SpriteRenderer>().sprite;
        transform.position = moveTarget.transform.position;
        transform.rotation = moveTarget.transform.rotation;
        transform.localScale = moveTarget.transform.localScale;
        isBeingMoved = true;
    }

    // Update is called once per frame
    void Update()
    {
        
        moveTargetPosition = DecorationController.Instance.DecorationSelector.transform.position;
        if (Vector3.Distance(transform.position, moveTargetPosition) > 0.01) //If the object is currently being moved, or hasn't reached their final placement after being placed
        { 
            transform.position = Vector3.Lerp(transform.position, moveTargetPosition, moveSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = moveTargetPosition; // If the object is close to it's target position, just snap too it rather than moving exponentially slower
            if (isPlaced)
            {
                // Send out signal that furniture should be moved
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
        Vector2 placementColliderPosition = new Vector2(transform.position.x, transform.position.y) + moveTarget.GetComponent<BoxCollider2D>().offset;
        Vector2 placementColliderSize = moveTarget.GetComponent<BoxCollider2D>().size;
    
        Collider2D[] placementCollisionCheckResults = Physics2D.OverlapBoxAll(placementColliderPosition, placementColliderSize, 0.0f); // ADD ROTATION // Checks if any other decorations or a platform is overlapping the decoration
        if (placementCollisionCheckResults.Length == 0 || (placementCollisionCheckResults.Length == 1 && placementCollisionCheckResults[0].gameObject == moveTarget)) // Checks if target location is overlapping other furniture or any platforms, ignoring itself
        {
            int successfulAttachPoints = 0;
            
            foreach (Vector3 _attachPointLocal in moveTarget.GetComponent<DecorationObject>().AttachmentPointsList) // Goes through each attach point
            {
                
                Vector2 attachPointGlobal = _attachPointLocal + transform.position;
                if (Physics2D.OverlapCircle(attachPointGlobal, moveTarget.GetComponent<DecorationObject>().AttachmentPointRadius, collisionCheckLayerMask))
                {
                    successfulAttachPoints++;
                }
                if (successfulAttachPoints == moveTarget.GetComponent<DecorationObject>().AttachmentPointsList.Count) // Checks if all attach points are valid
                {
                    placeable = true;
                }
            }
        }
        return placeable;
    }
}