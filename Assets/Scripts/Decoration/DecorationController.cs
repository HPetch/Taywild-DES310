using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DecorationController : MonoBehaviour
{

    public event Action OnEnterEditMode;
    public event Action OnExitEditMode;
    public event Action OnPickupDecoration;
    public event Action<FurnitureObject> OnPlaceDecoration;
    public event Action OnPlaceCancelDecoration;
    public event Action OnFurnitureDestroyed;
    public event Action OnPickupDamaged;
    public event Action OnPickupBroken;
    public event Action OnPickupCancel;

    public static DecorationController Instance { get; private set; }
    [SerializeField] private GameObject decorationSelectorPrefab;
    [SerializeField] private GameObject decorationMovingFakePrefab;
    public GameObject CurrentMoveTarget { get; private set; }
    public GameObject CurrentMoveFake { get; private set; }
    [field: SerializeField] public GameObject DecorationSelector { get; private set; }
    public bool isEditMode { get; private set; }

    [field: SerializeField] public SerializableDictionary<GameObject, bool> FurnitureObjectPrefabs { get; private set; }

    [SerializeField] private GameObject PP;

    public enum UiFurnitureCategories {INDOOR, OUTDOOR, LIGHTING, IDK, SOMETHINGELSE}

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

    public bool CanCraftFurniture(GameObject _furniturePrefab)
    {
        bool _canCraft = true;
        foreach (KeyValuePair<InventoryController.ItemNames, int> _item in _furniturePrefab.GetComponent<FurnitureObject>().CraftingRequirements)
        {
            if (InventoryController.Instance.ItemQuantity(_item.Key) < _item.Value) _canCraft = false;
        }
        return _canCraft;
    }
    
    public void SpawnFurniture(GameObject _furniturePrefab)
    {
        bool _canCraft = true;
        foreach (KeyValuePair<InventoryController.ItemNames, int> _item in _furniturePrefab.GetComponent<FurnitureObject>().CraftingRequirements)
        {
            if (InventoryController.Instance.ItemQuantity(_item.Key) < _item.Value) _canCraft = false;
        }
        if (_canCraft)
        {
            foreach (KeyValuePair<InventoryController.ItemNames, int> _item in _furniturePrefab.GetComponent<FurnitureObject>().CraftingRequirements)
            {
                InventoryController.Instance.RemoveItem(_item.Key, _item.Value, Vector2.zero);
                TreeLevelController.Instance.UIref.UpdateMaterialText(_item.Key, -_item.Value);
            }
            GameObject _newFurniture = Instantiate(_furniturePrefab);
            DecorationController.Instance.SelectorDecorationObjectInteract(_newFurniture, true);
        }
        else print("Failed to craft: " + _furniturePrefab);
    }

    public void SelectorDecorationObjectInteract(GameObject _selectedObject, bool _isMouseClickDown)
    {
        if (_isMouseClickDown)
        {
            if (_selectedObject.GetComponent<FurnitureObject>() && _selectedObject != CurrentMoveTarget)
            {
                DecorationMoveStart(_selectedObject);
            }
            else if (_selectedObject.GetComponent<PickupObject>())
            {
                _selectedObject.GetComponent<PickupObject>().StartPull();
            }
        }
        else
        {
            if (_selectedObject.GetComponent<DecorationButton>())
            {
                DecorationButtonPress(_selectedObject);
            }
        }
       
        
        
    }
    
    public void DecorationMoveStart(GameObject _decorationObject)
    {
        CurrentMoveTarget = _decorationObject;
        CurrentMoveTarget.GetComponent<FurnitureObject>().StartPickup();
        CurrentMoveFake = Instantiate(decorationMovingFakePrefab);
        CurrentMoveFake.GetComponent<DecorationMovingFake>().scrollRotateIndex = CurrentMoveTarget.GetComponent<FurnitureObject>().scrollRotateIndexHolder;
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
        CurrentMoveTarget.GetComponent<FurnitureObject>().SetScrollRotateIndexHolder(CurrentMoveFake.GetComponent<DecorationMovingFake>().scrollRotateIndex);
        Destroy(CurrentMoveFake);
        CurrentMoveFake = null;
        CurrentMoveTarget.GetComponent<FurnitureObject>().EndPickup();
        OnPlaceDecoration?.Invoke(CurrentMoveTarget.GetComponent<FurnitureObject>());
        CurrentMoveTarget = null;
    }

    private void DecorationMoveCancel()
    {
        if (CurrentMoveFake)
        {
            if (!CurrentMoveTarget.GetComponent<FurnitureObject>().isFirstTimePlace)
            {
                Destroy(CurrentMoveFake);
                CurrentMoveFake = null;
                CurrentMoveTarget.GetComponent<FurnitureObject>().EndPickup();
                CurrentMoveTarget = null;
                OnPlaceCancelDecoration?.Invoke();
            }
            else
            {
                Destroy(CurrentMoveFake);
                DestroyFurniture(CurrentMoveTarget);
            }
            
        }
        
    }

    public void DestroyFurniture(GameObject _furniture)
    {
        foreach (KeyValuePair<InventoryController.ItemNames, int> _item in _furniture.GetComponent<FurnitureObject>().CraftingRequirements)
        {
            InventoryController.Instance.AddItem(_item.Key, _item.Value, _furniture.transform.position);
        }
        TreeLevelController.Instance.RemoveFurnitureExp(_furniture.GetComponent<FurnitureObject>().treeExp);
        Destroy(_furniture);
        OnFurnitureDestroyed?.Invoke();

    }

    public void PickupDamaged(SerializableDictionary<InventoryController.ItemNames, Vector2Int> _itemsReceived, Vector2 _position)
    {
        PickupAddItems(_itemsReceived, _position);
        OnPickupDamaged?.Invoke();
    }

    public void PickupBroken(SerializableDictionary<InventoryController.ItemNames, Vector2Int> _itemsReceived, Vector2 _position)
    {
        PickupAddItems(_itemsReceived, _position);
        OnPickupBroken?.Invoke();
    }

    public void PickupCancel()
    {
        OnPickupCancel?.Invoke();
    }

    private void PickupAddItems(SerializableDictionary<InventoryController.ItemNames, Vector2Int> _itemsReceived, Vector2 _position)
    {
        foreach (KeyValuePair<InventoryController.ItemNames, Vector2Int> _item in _itemsReceived) // Go through each item that the pickup dropped
        {
            int _itemAmount = 0;
            if (_item.Value.y > 0 && _item.Value.y > _item.Value.x) _itemAmount = UnityEngine.Random.Range(_item.Value.x, _item.Value.y); // Randomise the amount dropped using the min and max
            else _itemAmount = _item.Value.x; // If the max is 0 or lower than the min then just use the min instead
            InventoryController.Instance.AddItem(_item.Key, _itemAmount, _position); // Add the item and it's amount to the inventory
        }
    }

    public void DecorationButtonPress(GameObject _button)
    {
        if (_button.GetComponentInParent<FurnitureObject>())
        {
            FurnitureObject _furnitureObject = _button.GetComponentInParent<FurnitureObject>();
            if (_button == _furnitureObject.RemoveButton) DestroyFurniture(_furnitureObject.gameObject); // Pickup object and refund materials to inventory
            else if (_button == _furnitureObject.EditButtonLeft) Debug.Log(_furnitureObject.gameObject + "Has gone to previous style");
            else if (_button == _furnitureObject.EditButtonRight) Debug.Log(_furnitureObject.gameObject + "Has gone to next style");
        }
        
        
    }

    

    // Event: Enter edit mode
    // Instantiate decoration selector

    private void ToggleEditMode()
    {
        if (isEditMode && !CurrentMoveFake && CurrentMoveFake == null)
        {
            CameraController.Instance.ResetTarget();
            DecorationMoveCancel();
            isEditMode = false;
            Destroy(DecorationSelector);
            OnExitEditMode?.Invoke();
            PP.SetActive(false);
            
        }
        else if(!isEditMode && PlayerController.Instance.IsGrounded && !DialogueController.Instance.IsConversing)
        {
            DecorationSelector = Instantiate(decorationSelectorPrefab);
            CameraController.Instance.SetTarget(DecorationSelector.transform);
            isEditMode = true;
            OnEnterEditMode?.Invoke();
            PP.SetActive(true);
        }
    }
}
