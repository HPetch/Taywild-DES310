using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }

    #region Events
    public event Action<int> OnAddItem; // Called when an item is added, returns the new total
    public event Action<int> OnRemoveItem; // Called when an item is attempted be be subtracted, returns new value and weither the subtraction worked or not
    public event Action<int> OnAmountChangeItem; // Called when item amount changes, ignores if item was added or subtracted, returns the new total
    #endregion

    #region Public variables
    public enum ItemNames // Should be set to { get; private set; }
    {
        FLOWER, TWIG, ROOT
    }
    
    public Dictionary<ItemNames, int> ItemDictionary { get; private set; }
    #endregion

    #region Functions

    #region Initialisation
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public void AddItem(ItemNames _item, int _amount)
    {

        if (_amount > 0)
        {
            Mathf.Clamp(ItemDictionary[_item] += _amount, 0, 100);
            OnAmountChangeItem.Invoke(ItemDictionary[_item]);
            OnAddItem.Invoke(ItemDictionary[_item]);
        }
        else Debug.LogWarning("Attempted to add item, but amount wasn't provided or was negative. Amount: " + _amount);
        
    }

    public void RemoveItem(ItemNames _item, int _amount)
    {
        if (_amount != 0)
        {
            bool isEnough = (ItemDictionary[_item] + math.abs(_amount)) > 0;
            if (isEnough)
            {
                Mathf.Clamp(ItemDictionary[_item] -= math.abs(_amount), 0, 100);
                OnAmountChangeItem.Invoke(ItemDictionary[_item]);
                if (ItemDictionary[_item] == 0)
                {
                    ItemDictionary.Remove(_item);
                }
            }
            OnRemoveItem.Invoke(ItemDictionary[_item]);
        }
        else Debug.LogWarning("Attempted to remove item, but no amount to remove was provided. Amount:" + _amount);
    }
    #endregion



}