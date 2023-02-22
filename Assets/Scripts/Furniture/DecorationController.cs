using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DecorationController : MonoBehaviour
{

    public event Action OnEnterEditMode;
    public event Action OnExitEditMode;
    public event Action OnPickupDecoration;
    public event Action OnPlaceDecoration;
    public event Action OnPlaceCancelDecoration;

    public static DecorationController Instance { get; private set; }
    [SerializeField] private GameObject decorationSelectorPrefab;
    [SerializeField] private GameObject decorationMovingFakePrefab;
    public GameObject CurrentMoveTarget { get; private set; }
    public GameObject CurrentMoveFake { get; private set; }
    [field: SerializeField] public GameObject DecorationSelector { get; private set; }
    public bool isEditMode { get; private set; }
    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isEditMode)
        {
            if (!CurrentMoveFake) // If no decoration is being moved, normal interaction mode
            {
            
            }
            else // If a decoration is being moved, cursor shouldn't interact with anything else
            {
                if (CurrentMoveFake.GetComponent<DecorationMovingFake>().CheckIfPlaceable()) // If the movement fake is in a viable placement location
                {
                    if(Input.GetMouseButtonUp(0))
                    {
                        DecorationMoveEndStart(); // Delete the movement fake and place the actual decoration in it's place
                    }
                }

                if (Input.GetMouseButtonDown(1))
                {
                    DecorationMoveCancel();
                }

                if (Input.GetAxis("Mouse ScrollWheel") != 0f)
                {
                    CurrentMoveFake.GetComponent<DecorationMovingFake>().scrollRotate(Input.GetAxis("Mouse ScrollWheel") > 0f);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleEditMode();
        }
        
    }


    public void SelectorDecorationObjectInteract(GameObject _selectedObject)
    {
        if (_selectedObject.GetComponent<DecorationObject>() && _selectedObject != CurrentMoveTarget )
        {
            DecorationMoveStart(_selectedObject);
        }
        else if (_selectedObject.GetComponent<DecorationButton>())
        {
            DecorationButtonPress(_selectedObject);
        }
    }
    
    public void DecorationMoveStart(GameObject _decorationObject)
    {
        CurrentMoveTarget = _decorationObject;
        CurrentMoveTarget.GetComponent<DecorationObject>().StartPickup();
        CurrentMoveFake = Instantiate(decorationMovingFakePrefab);
        CurrentMoveFake.GetComponent<DecorationMovingFake>().scrollRotateIndex = CurrentMoveTarget.GetComponent<DecorationObject>().scrollRotateIndexHolder;
        OnPickupDecoration?.Invoke();
    }

    private void DecorationMoveEndStart()
    {
        CurrentMoveFake.GetComponent<DecorationMovingFake>().placeDecorationFake();
    }
    public void DecorationMoveEndFinish()
    {
        CurrentMoveTarget.transform.position = CurrentMoveFake.transform.position;
        CurrentMoveTarget.transform.rotation = CurrentMoveFake.transform.rotation;
        CurrentMoveTarget.GetComponent<DecorationObject>().SetScrollRotateIndexHolder(CurrentMoveFake.GetComponent<DecorationMovingFake>().scrollRotateIndex);
        Destroy(CurrentMoveFake);
        CurrentMoveFake = null;
        CurrentMoveTarget.GetComponent<DecorationObject>().EndPickup();
        CurrentMoveTarget = null;
        OnPlaceDecoration?.Invoke();
    }

    private void DecorationMoveCancel()
    {
        if (CurrentMoveFake)
        {
            Destroy(CurrentMoveFake);
            CurrentMoveFake = null;
            CurrentMoveTarget.GetComponent<DecorationObject>().EndPickup();
            CurrentMoveTarget = null;
            OnPlaceCancelDecoration?.Invoke();
        }
        
    }

    public void DecorationButtonPress(GameObject _button)
    {
        DecorationObject _decorationObject = _button.GetComponentInParent<DecorationObject>();
        if (_button == _decorationObject.PickupButton) Debug.Log(_decorationObject.gameObject + "Has been picked up into the inventory"); // Pickup object and refund materials to inventory
        if (_button == _decorationObject.EditButtonLeft) Debug.Log(_decorationObject.gameObject + "Has gone to previous style");
        if (_button == _decorationObject.EditButtonRight) Debug.Log(_decorationObject.gameObject + "Has gone to next style");
    }

    // Event: Enter edit mode
    // Instantiate decoration selector

    private void ToggleEditMode()
    {
        if (isEditMode)
        {
            DecorationMoveCancel();
            isEditMode = false;
            Destroy(DecorationSelector);
            OnEnterEditMode?.Invoke();
        }
        else
        {
            DecorationSelector = Instantiate(decorationSelectorPrefab);
            isEditMode = true;
            OnExitEditMode?.Invoke();
        }
    }
}
