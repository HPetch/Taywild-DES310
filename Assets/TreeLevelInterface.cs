using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TreeLevelInterface : MonoBehaviour
{
    #region Object references + variables
    [SerializeField] private GameObject[] SpriteAtLevel; //What sprite should be shown at each level (0 should be invisible, 1,2,3 relevant cherry blossom etc)
    [SerializeField] private TMP_Text[] MaterialTexts; //Text components that update

    [Header("Open/close animation")]
    private bool isOpen = false;
    [SerializeField] private RectTransform interfaceParent; //What element needs moving
    [SerializeField] private float positionOpened = 16f; //X axis in Rect Transform to go to when opening.
    private float positionClosed; //X axis position to go to when closing
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private LeanTweenType OpenAnimation;
    [SerializeField] private LeanTweenType CloseAnimation;
    #endregion

    #region Methods

    private void Start()
    {
        positionClosed = interfaceParent.anchoredPosition.x; //Get closed position based on where we put it at the start.
    }

    //Update relevant text with relevant number
    private void UpdateMaterialText(InventoryController.ItemNames type, int value)
    {
        //Find any texts that have the item type, and update their text component if so
        foreach (TMP_Text item in MaterialTexts)
        {
            if (item.GetComponent<TreeLevelMaterialType>().MaterialType == type)
            {
                //Update text with value here
                item.text = value.ToString();
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

    #endregion
}
