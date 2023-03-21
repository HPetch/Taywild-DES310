// Inventory Controller
// Uses a dictionary to track each item the player has stored.
// Items can be added and removed with subsequent relevent events being fired
// The OnItemQuantityChanged event is fired on both operations

using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public enum ItemNames { LEAF, WOOD, SAP , QUEST_BLUEFLOWER}

    public static InventoryController Instance { get; private set; }

    #region Events
    public event Action<ItemNames, int, Vector2> OnItemAdded; 
    public event Action<ItemNames, int, Vector2> OnItemRemoved; 
    public event Action<ItemNames, int> OnItemQuantityChanged;
    #endregion

    #region Variables
    // The dictionary that represents the inventory
    private Dictionary<ItemNames, int> inventory = new Dictionary<ItemNames, int>();
    [SerializeField] private GameObject itemDebrisPrefab;
    #endregion

    #region Functions
    #region Initialisation
    private void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_item"> The item that you wish to enquire about</param>
    /// <returns> The quantity of the specified item in the inventory dictionary</returns>
    public int ItemQuantity(ItemNames _item)
    {
        if (InventoryDoesNotContainItem(_item)) return 0;
        else return inventory[_item];
    }

    /// <summary>
    /// Adds a quantity of a specified item to the inventory
    /// </summary>
    /// <param name="_item"> The item you wish to add</param>
    /// <param name="_quantity"> The quantity you wish to add</param>
    public void AddItem(ItemNames _item, int _quantity, Vector2 _position)
    {
        if (_quantity == 0) Debug.Log("A quantity of 0 was added");
        
        // Just in case a - quantity has been inputed
        _quantity = Mathf.Abs(_quantity);

        

        // If the item is already in the dictionary, then increment the quantity
        if (InventoryContainsItem(_item)) inventory[_item] += _quantity;
        // Else add the item to the dictionary
        else if (_quantity != 0) inventory.Add(_item, _quantity);
        
        if (_quantity != 0)
        {
            OnItemAdded?.Invoke(_item, inventory[_item], _position);
            OnItemQuantityChanged?.Invoke(_item, inventory[_item]);
            
            SpawnMaterialDebris(_item, _quantity, _position);
        }
        
    }

    /// <summary>
    /// Removes a quantity of a specified item from the inventory
    /// </summary>
    /// <param name="_item"> The item you wish to remove</param>
    /// <param name="_quantity"> The quantity you wish to remove</param>
    public void RemoveItem(ItemNames _item, int _quantity, Vector2 _location)
    {
        if (_quantity == 0) Debug.LogWarning("A quantity of 0 was removed");

        // Take the absolute so either a positive or negative will remove the correct amount
        _quantity = Mathf.Abs(_quantity);

        if (InventoryDoesNotContainItem(_item))
        {
            Debug.LogWarning("Tried to remove " + _item + " from inventory, but item isn't in inventory. Make sure to check first");
            return;
        }

        // If the inventory does not contain enough quantity of the item to remove the amount
        if (inventory[_item] - _quantity < 0)
        {
            Debug.LogWarning("Insufficient item quantity. Item: " + _item + "\nItem quantity: " + inventory[_item] + "\nQuantity attempted to remove: " + _quantity);
            return;
        }

        inventory[_item] -= _quantity;

        OnItemRemoved?.Invoke(_item, inventory[_item], _location);
        OnItemQuantityChanged?.Invoke(_item, inventory[_item]);

        if (inventory[_item] == 0) inventory.Remove(_item);
    }
    
    
    public void SpawnMaterialDebris(ItemNames _debrisName, int _debrisQuantity, Vector2 _position)
    {
        if (_position != Vector2.zero)
        {
            _debrisQuantity = Mathf.Abs(_debrisQuantity);
        
            for (int i = 0; i < _debrisQuantity; i++)
            {
                GameObject _debris = Instantiate(itemDebrisPrefab);
                _debris.GetComponent<DecorationPickupDebris>().initialize(_debrisName, _position, _debrisQuantity);
            }
            
        }
        
    
    }
    
    

    #region Utility functions
    private bool InventoryContainsItem(ItemNames _item) { return inventory.ContainsKey(_item); }
    private bool InventoryDoesNotContainItem(ItemNames _item) { return !inventory.ContainsKey(_item); }


    #endregion

    #region Context Menu Functions

    [ContextMenu("Print inventory")]
    private void PrintInventory()
    {
        foreach(KeyValuePair<ItemNames, int> _item in inventory)
        {
            Debug.Log("Item: " + _item.Key + " - " + _item.Value);
        }
    }
    #endregion

    #endregion
}