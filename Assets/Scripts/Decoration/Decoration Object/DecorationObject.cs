using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class DecorationObject : MonoBehaviour
{

    // SHOULD CHANGE TO INHERITANCE. Environment object should be base. Decoration object

    [SerializeField] protected GameObject pickupButtonPrefab;
    [SerializeField] protected GameObject editButtonHolderPrefab;

    public GameObject PickupButton { get; protected set; }
    public GameObject EditButtonHolder { get; protected set; }
    public GameObject EditButtonLeft { get; protected set; }
    public GameObject EditButtonRight { get; protected set; }

    protected bool isMoving;
    private bool isHovered;
    private float timeOfLastHover = 0f;


    private void Start()
    {
        
        EndHover();
        
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

    

    private void Reset()
    {
        /*
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
        }*/

        
    }
}
