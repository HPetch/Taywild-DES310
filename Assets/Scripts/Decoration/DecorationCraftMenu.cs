using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class DecorationCraftMenu : MonoBehaviour
{
    private List<GameObject> availableFurniture = new List<GameObject>();
    private DecorationCraftMenuButton[] buttonList;
    private DecorationCraftMenuButton selectedButton;

    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text resource1CostText;
    [SerializeField] private TMP_Text resource2CostText;
    [SerializeField] private TMP_Text resource3CostText;

    private DecorationController.UiFurnitureCategories activeCatagory = DecorationController.UiFurnitureCategories.INDOOR;
    [SerializeField] SerializableDictionary<Button, DecorationController.UiFurnitureCategories> categoryButtons;

    [SerializeField] private Button placeButton;

    private void Awake()
    {
        foreach (KeyValuePair<Button, DecorationController.UiFurnitureCategories> _button in categoryButtons)
        {
            _button.Key.GetComponent<Button>().onClick.AddListener(delegate { RebuildFurnitureMenu(_button.Value); });
        }

        buttonList = GetComponentsInChildren<DecorationCraftMenuButton>();

        foreach (DecorationCraftMenuButton _button in buttonList)
        {
            _button.GetComponent<Button>().onClick.AddListener(delegate { SelectButton(_button); });
        }

        
        placeButton.GetComponent<Button>().onClick.AddListener(delegate { PlaceFurniture(); });
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach(KeyValuePair<GameObject, bool> _furniture in DecorationController.Instance.FurnitureObjectPrefabs)
        {
            print(_furniture);
            print(_furniture.Key);
            print(_furniture.Value);
            if (_furniture.Value) availableFurniture.Add(_furniture.Key);
        }

        
        RebuildFurnitureMenu(DecorationController.UiFurnitureCategories.INDOOR);
    }
    

    


    private void RebuildFurnitureMenu(DecorationController.UiFurnitureCategories _category)
    {
        foreach (DecorationCraftMenuButton _button in buttonList)
        {
            _button.ResetButton();
        }
        activeCatagory = _category;
        foreach (KeyValuePair<GameObject, bool> _furniture in DecorationController.Instance.FurnitureObjectPrefabs)
        {
            FurnitureObject _furnitureScript = _furniture.Key.GetComponent<FurnitureObject>();
            if (_furniture.Value && _furnitureScript.UiFurnitureCategory == activeCatagory)
            {
                foreach (DecorationCraftMenuButton _button in buttonList)
                {
                    if (!_button.AssignedFurniture)
                    {
                        _button.SetFurniture(_furniture.Key);
                        Debug.Log("Set Furniture: " + _furniture.Key);
                        break;
                    }
                }
            }
        }
        SelectButton(buttonList[0]);
    }

    public void SelectButton(DecorationCraftMenuButton _button)
    {
        FurnitureObject _furnitureScript = null;
        selectedButton = null;
        nameText.text = "Crafting menu";
        int _resource1Amount = 0;
        int _resource2Amount = 0;
        int _resource3Amount = 0;

        if (_button && _button != null && _button.AssignedFurniture && _button.AssignedFurniture != null)
        {
            if (_button.AssignedFurniture.GetComponent<FurnitureObject>())
            {
                _furnitureScript = _button.AssignedFurniture.GetComponent<FurnitureObject>();

                nameText.text = _furnitureScript.UiName;


                foreach (KeyValuePair<InventoryController.ItemNames, int> _item in _furnitureScript.CraftingRequirements)
                {
                    if (_item.Key == InventoryController.ItemNames.FLOWER) _resource1Amount = _item.Value;
                    else if (_item.Key == InventoryController.ItemNames.TWIG) _resource2Amount = _item.Value;
                    else if (_item.Key == InventoryController.ItemNames.ROOT) _resource3Amount = _item.Value;
                }
                selectedButton = _button;
                Debug.Log(_button);
            }
        }
        resource1CostText.text = _resource1Amount.ToString();
        resource2CostText.text = _resource2Amount.ToString();
        resource3CostText.text = _resource3Amount.ToString();
    }

    private void PlaceFurniture()
    {
        Debug.Log("Try Place");
        if (selectedButton && selectedButton != null)
        {
            if (selectedButton.AssignedFurniture.GetComponent<FurnitureObject>())
            {
                Debug.Log("Spawn: " + selectedButton.AssignedFurniture.GetComponent<FurnitureObject>().UiName);
                //Close DecorationCraftMenu. Run function that is normally called when clicking the pull tab.
                //DecorationController.Instance.SpawnFurniture(AssignedFurniture);
            }
        }
    }


}
