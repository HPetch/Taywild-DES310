using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TreeLevelInterface : MonoBehaviour
{
    #region Object references + variables
    [SerializeField] private GameObject[] SpriteAtLevel; //What sprite should be shown at each level (0 should be invisible, 1,2,3 relevant cherry blossom etc)
    [SerializeField] private TMP_Text[] MaterialTexts; //Text components that update
    [field: SerializeField] public Transform LeafSprite { get; private set; }
    [field: SerializeField] public Transform WoodSprite { get; private set; }
    [field: SerializeField] public Transform SapSprite { get; private set; }

    [Header("Open/close animation")]
    private bool isOpen = false;
    [SerializeField] private RectTransform interfaceParent; //What element needs moving
    [SerializeField] private float positionOpened = 16f; //X axis in Rect Transform to go to when opening.
    private float positionClosed; //X axis position to go to when closing
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private LeanTweenType OpenAnimation;
    [SerializeField] private LeanTweenType CloseAnimation;
    [SerializeField, Range(4, 20)] private int CloseDelay;
    private float closeTime;

    #endregion

    

    #region Methods

    private void Start()
    {
        positionClosed = interfaceParent.anchoredPosition.x; //Get closed position based on where we put it at the start.
        DecorationController.Instance.OnEnterEditMode += Open;
        InventoryController.Instance.OnItemQuantityChanged += OpenItem;
    }

    private void Update()
    {
        if (closeTime < Time.time && !DecorationController.Instance.isEditMode) Close();
    }

    //Update relevant text with relevant number
    public void UpdateMaterialText(InventoryController.ItemNames type, int value)
    {
        //Find any texts that have the item type, and update their text component if so
        foreach (TMP_Text item in MaterialTexts)
        {
            if (item.GetComponent<TreeLevelMaterialType>().MaterialType == type)
            {
                //Update text with value here
                item.text = (int.Parse(item.text) + value).ToString();
            }
        }
    }
    
    //Slide the interface onto the screen
    private void Open()
    {
        if (!isOpen)
        {
            LeanTween.cancel(interfaceParent); //Cancel anything that may be happening
            LeanTween.moveX(interfaceParent, positionOpened, animationDuration).setEase(OpenAnimation);
            isOpen = true;
            ResetTimer(1);
        }
        else ResetTimer(1);
    }
    private void OpenItem(InventoryController.ItemNames _item, int _quantity)
    {
        if (_item == InventoryController.ItemNames.LEAF || _item == InventoryController.ItemNames.WOOD || _item == InventoryController.ItemNames.SAP)
        {
            Open();
            ResetTimer(1.5f);
        }
    }

    //Slide the interface off of the screen
    private void Close()
    {
        if (isOpen)
        {
            LeanTween.cancel(interfaceParent); //Cancel anything that may be happening
            LeanTween.moveX(interfaceParent, positionClosed, animationDuration).setEase(CloseAnimation);
            isOpen = false;
        }
    }


    private void ResetTimer(float _multiplier)
    {
        if (closeTime < Time.time + (CloseDelay * _multiplier)) closeTime = Time.time + (CloseDelay * _multiplier);

    }
    #endregion
}
