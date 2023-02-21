using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }

    #region Events
    public event Action<ItemNames, int> OnAddItem; // Called when an item is added, returns the new total
    public event Action<ItemNames, int> OnRemoveItem; // Called when an item is attempted be be subtracted, returns new value and weither the subtraction worked or not
    public event Action<ItemNames, int> OnAmountChangeItem; // Called when item amount changes, ignores if item was added or subtracted, returns the new total
    #endregion

    #region Public variables
    public enum ItemNames // Should be set to { get; private set; }
    {
        FLOWER, TWIG, ROOT
    }

    private Dictionary<ItemNames, int> inventory = new Dictionary<ItemNames, int>();
    
    #endregion

    #region Functions

    #region Initialisation
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public int CheckItem(ItemNames _item)
    {
        if (InventoryDoesNotContainItem(_item)) return 0;
        else return inventory[_item];
    }

    public void AddItem(ItemNames _item, int _amount)
    {
        if (_amount > 0)
        {
            if (InventoryDoesNotContainItem(_item))
            {
                inventory.Add(_item, _amount);
            }
            else
            {
                Mathf.Clamp(inventory[_item] += _amount, 0, 100);
            }
            OnAmountChangeItem?.Invoke(_item, inventory[_item]);
            OnAddItem?.Invoke(_item, inventory[_item]);
        }
        else Debug.LogWarning("Attempted to add item, but amount wasn't provided or was negative. Amount: " + _amount);
        if (inventory[_item] >= 99) Debug.LogWarning("Maximum item value reached. Somthing must have gone wrong. Item: " + _item);
    }

    public void RemoveItem(ItemNames _item, int _amount)
    {
        if (InventoryContainsItem(_item))
        {
            if (inventory[_item] - math.abs(_amount) > 0 && _amount != 0) // Checks that there are enough items to remove, and if there was a number to subtract provided
            {
                Mathf.Clamp(inventory[_item] -= math.abs(_amount), 0, 100);
                OnAmountChangeItem?.Invoke(_item, inventory[_item]);
                OnRemoveItem?.Invoke(_item, inventory[_item]);
                if (inventory[_item] == 0) inventory.Remove(_item);
            }
            else Debug.LogWarning("Attempted to remove item, but no amount to remove was provided or not enough items remaining. Amount:" + _amount + ". Item: " + _item + ": " + inventory[_item]);
        }
        else Debug.LogWarning("Tried to remove item from inventory, but item isn't in inventory. Make sure to run CheckItem first");
    }
    
    #region Utility functions
    private bool InventoryContainsItem(ItemNames _item) { return inventory.ContainsKey(_item); }
    private bool InventoryDoesNotContainItem(ItemNames _item) { return !inventory.ContainsKey(_item); }
    #endregion
    #endregion
}