using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class DecorationObject : MonoBehaviour
{
    public enum ObjectType {DECORATION_OBJECT, ENVIROMENT_OBJECT, TRASH_OBJECT} //Decoration object - Move Edit Delete, Enviroment - Edit, Trash - Delete
    public ObjectType objectType;
    [field: SerializeField] public List<Vector3> AttachmentPointsList{ get; private set; }
    public float AttachmentPointRadius { get; private set; }
    [field: SerializeField] public int scrollRotateIndexHolder { get; private set; }

    [SerializeField] private GameObject pickupButtonPrefab;
    [SerializeField] private GameObject editButtonHolderPrefab;

    [field: SerializeField] public GameObject PickupButton { get; private set; }
    [field: SerializeField] public GameObject EditButtonHolder { get; private set; }
    [field: SerializeField] public GameObject EditButtonLeft { get; private set; }
    [field: SerializeField] public GameObject EditButtonRight { get; private set; }

    private bool isMoving;
    private bool isHovered;
    private float timeOfLastHover = 0f;


    private void Start()
    {
        AttachmentPointRadius = GetComponentInChildren<CircleCollider2D>().radius;
        EndHover();
        if(PickupButton) PickupButton.transform.position = GetComponent<BoxCollider2D>().bounds.max;
        if(EditButtonHolder) EditButtonHolder.transform.position = new Vector2(transform.position.x, GetComponent<BoxCollider2D>().bounds.min.y);
    }

    private void Update()
    {
        if ( Time.time - timeOfLastHover > 1f || isMoving) EndHover();
    }

    public void StartHover()
    {
        if (!isMoving)
        {
            if (PickupButton) ShowButton(PickupButton);
            if (EditButtonLeft) ShowButton(EditButtonLeft);
            if (EditButtonRight) ShowButton(EditButtonRight);
            timeOfLastHover = Time.time;
            isHovered = true;
        }
        else { EndHover(); }
    }

    public void EndHover()
    {
        if (PickupButton) HideButton(PickupButton);
        if (EditButtonLeft) HideButton(EditButtonLeft);
        if (EditButtonLeft) HideButton(EditButtonRight);
        isHovered = false;
    }

    public void StartPickup()
    {
        isMoving = true;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.25f);
    }
    public void EndPickup()
    {
        isMoving = false;
        GetComponent<SpriteRenderer>().color = Color.white;
    }


    private void HideButton(GameObject _button)
    {
        if (_button.GetComponent<DecorationButton>())
        {

            _button.GetComponent<SpriteRenderer>().color = Color.clear;
            _button.GetComponent<BoxCollider2D>().enabled = false;
        }

    }
    private void ShowButton(GameObject _button)
    {
        if (_button.GetComponent<DecorationButton>())
        {
            _button.GetComponent<SpriteRenderer>().color = Color.white;
            _button.GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    public void SetScrollRotateIndexHolder(int _index) { scrollRotateIndexHolder = _index; }

    private void Reset()
    {
        foreach (Transform _attachmentPoint in transform)
        {
            
            if (_attachmentPoint.gameObject.tag == "Decoration Attach Point")
            {
                AttachmentPointsList.Add(_attachmentPoint.localPosition);
                AttachmentPointRadius = _attachmentPoint.GetComponent<CircleCollider2D>().radius;
            }
        }
        switch (objectType)
        {
            case ObjectType.DECORATION_OBJECT:
                if (!PickupButton) Instantiate(pickupButtonPrefab);
                if (!EditButtonHolder) Instantiate(editButtonHolderPrefab);
                EditButtonLeft = EditButtonHolder.transform.GetChild(0).gameObject;
                EditButtonRight = EditButtonHolder.transform.GetChild(1).gameObject;
                break;
            case ObjectType.ENVIROMENT_OBJECT:
                if (PickupButton) Destroy(pickupButtonPrefab);
                if (!EditButtonHolder) Instantiate(editButtonHolderPrefab);
                EditButtonLeft = EditButtonHolder.transform.GetChild(0).gameObject;
                EditButtonRight = EditButtonHolder.transform.GetChild(1).gameObject;
                break;
            case ObjectType.TRASH_OBJECT:
                if (!PickupButton) Instantiate(pickupButtonPrefab);
                if (EditButtonHolder) Destroy(editButtonHolderPrefab);
                break;
        }

        PickupButton.transform.position = GetComponent<BoxCollider2D>().bounds.max;
        EditButtonHolder.transform.position = new Vector2 (transform.position.x, GetComponent<BoxCollider2D>().bounds.min.y);
    }
}
