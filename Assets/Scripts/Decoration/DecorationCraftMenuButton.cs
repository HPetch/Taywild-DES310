using UnityEngine.UI;
using UnityEngine;

public class DecorationCraftMenuButton : MonoBehaviour
{
    
    public GameObject AssignedFurniture { get; private set; }

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(delegate { SetFurniture(AssignedFurniture); });
    }





    public void SetFurniture(GameObject _furniturePrefab)
    {
        if(_furniturePrefab && _furniturePrefab != null)
        {
            AssignedFurniture = _furniturePrefab;
            FurnitureObject _furnitureScript = _furniturePrefab.GetComponent<FurnitureObject>();
            transform.GetChild(0).GetComponent<Image>().sprite = _furnitureScript.UiImage;
        }
    }
    public void ResetButton()
    {
        AssignedFurniture = null;
        transform.GetChild(0).GetComponent<Image>().sprite = null;
    }





}
