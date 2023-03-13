using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationCraftMenu : MonoBehaviour
{
    private List<GameObject> availableFurniture = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        foreach(KeyValuePair<GameObject, bool> _furniture in DecorationController.Instance.PlaceableDecorationObjectPrefabs)
        {
            print(_furniture);
            print(_furniture.Key);
            print(_furniture.Value);
            if (_furniture.Value) availableFurniture.Add(_furniture.Key);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SpawnFurniture(availableFurniture[0]);
    }

    

    private void SpawnFurniture(GameObject _furniturePrefab)
    {
        bool _canCraft = true;
        foreach(KeyValuePair<InventoryController.ItemNames, int> _item in _furniturePrefab.GetComponent<FurnitureObject>().CraftingRequirements)
        {
            if (InventoryController.Instance.ItemQuantity(_item.Key) < _item.Value) _canCraft = false;
        }
        if (_canCraft)
        {
            foreach (KeyValuePair<InventoryController.ItemNames, int> _item in _furniturePrefab.GetComponent<FurnitureObject>().CraftingRequirements)
            {
                InventoryController.Instance.RemoveItem(_item.Key, _item.Value);
            }
            print("Crafted: " + _furniturePrefab);
            // Close menu
            // Spawn furniture grabbed by selector
            GameObject _newFurniture = Instantiate(_furniturePrefab);
            DecorationController.Instance.SelectorDecorationObjectInteract(_newFurniture, true);
        }
        else print("Failed to craft: " + _furniturePrefab);
    }

}
