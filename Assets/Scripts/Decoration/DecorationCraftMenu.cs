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

    //Inventory drawer management
    [Header("Open and close tab")]
    [SerializeField] private GameObject drawer;
    private RectTransform drawerTransform;

    private Vector3 closedPos;
    private Vector3 openPos;

    [SerializeField] private Button pullTab;
    [SerializeField] private GameObject pullTabArrow;
    private bool inventoryIsOpen = false;

    [SerializeField] private float positionOpened = 50f;
    [SerializeField] private float duration = 0.5f;

    [SerializeField] private LeanTweenType openEase;
    [SerializeField] private LeanTweenType closeEase;

    [Header("Audio")]
    [SerializeField] private AudioClip inventoryOpen;
    [SerializeField] private AudioClip inventoryClose;
    [SerializeField] private AudioClip craftClip;


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
        foreach (KeyValuePair<GameObject, bool> _furniture in DecorationController.Instance.FurnitureObjectPrefabs)
        {
            if (_furniture.Value) availableFurniture.Add(_furniture.Key);
        }


        RebuildFurnitureMenu(DecorationController.UiFurnitureCategories.INDOOR);

        //Get drawerTransform
        drawerTransform = drawer.GetComponent<RectTransform>();
        closedPos = drawerTransform.anchoredPosition;
        openPos = drawerTransform.anchoredPosition + new Vector2(0, positionOpened);
        //Trigger ToggleInventory when the pull up/down tab is pressed
        pullTab.onClick.AddListener(ToggleInventory);
    }

    private void Update()
    {
        if (selectedButton)
        {
            if (DecorationController.Instance.CanCraftFurniture(selectedButton.AssignedFurniture)) placeButton.GetComponent<Image>().color = Color.white;
            else placeButton.GetComponent<Image>().color = Color.black;
        }
        else placeButton.GetComponent<Image>().color = Color.black;

    }

    //Check current state and either open or close the inventory UI
    private void ToggleInventory()
    {
        //Either open or close the inventory panel
        
         if (!inventoryIsOpen) OpenInventory(); else CloseInventory();
        
        
    }

    private LTDescr OpenInventory()
    {
        
        LeanTween.cancel(gameObject); //Stop current tweens
        LeanTween.cancel(pullTabArrow); //Stop arrow if it is mid-tween

        inventoryIsOpen = true; //Mark the inventory as opening
        AudioController.Instance.PlaySound(inventoryOpen);//Play sound
        LeanTween.rotateZ(pullTabArrow,180.5f,duration / 2).setEase(LeanTweenType.easeInOutSine); //Rotate arrow alongside actual move (180.5 guarantees it will always rotate clockwise)
        return LeanTween.move(drawerTransform, openPos, duration).setEase(openEase); //Go to open position
    }

    private LTDescr CloseInventory()
    {
        LeanTween.cancel(gameObject); //Stop current tweens if mid-open
        LeanTween.cancel(pullTabArrow); //Stop arrow if it is mid-tween

        inventoryIsOpen = false; //Mark the inventory as opening
        AudioController.Instance.PlaySound(inventoryClose);
        LeanTween.rotateZ(pullTabArrow, 0f, duration / 2).setEase(LeanTweenType.easeInOutSine); //Rotate arrow alongside actual move
        return LeanTween.move(drawerTransform, closedPos, duration).setEase(closeEase); //Go to close position
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
                    if (_item.Key == InventoryController.ItemNames.LEAF) _resource1Amount = _item.Value;
                    else if (_item.Key == InventoryController.ItemNames.WOOD) _resource2Amount = _item.Value;
                    else if (_item.Key == InventoryController.ItemNames.SAP) _resource3Amount = _item.Value;
                }
                selectedButton = _button;
            }
        }
        resource1CostText.text = _resource1Amount.ToString();
        resource2CostText.text = _resource2Amount.ToString();
        resource3CostText.text = _resource3Amount.ToString();
    }

    private void PlaceFurniture()
    {
        if (selectedButton && selectedButton != null && (!DecorationController.Instance.CurrentMoveFake || DecorationController.Instance.CurrentMoveFake == null))
        {
            if (selectedButton.AssignedFurniture.GetComponent<FurnitureObject>())
            {
                if (DecorationController.Instance.CanCraftFurniture(selectedButton.AssignedFurniture))
                {
                    CloseInventory();
                    DecorationController.Instance.SpawnFurniture(selectedButton.AssignedFurniture);
                    AudioController.Instance.PlaySound(craftClip);
                }
                else Debug.Log("Not enough to craft");
            }
            else Debug.Log("No object selected");
        }
        else Debug.Log("No button selected");
    }


}
