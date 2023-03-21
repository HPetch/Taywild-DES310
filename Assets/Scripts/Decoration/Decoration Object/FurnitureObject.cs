using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureObject : DecorationObject
{
    [field: SerializeField] public SerializableDictionary<InventoryController.ItemNames, int> CraftingRequirements { get; private set; }



    public List<Vector3> AttachmentPointsList { get; private set; } = new List<Vector3>();
    public float AttachmentPointRadius { get; private set; }
    [field: SerializeField] public int scrollRotateIndexHolder { get; private set; }

    bool isFirstTimePlace = true;
    [field: SerializeField] public int treeExp { get; private set; }

    [field: SerializeField] public string UiName { get; private set; }
    [field: SerializeField] public Sprite UiImage { get; private set; }
    [field: SerializeField] public DecorationController.UiFurnitureCategories UiFurnitureCategory { get; private set; }








// Start is called before the first frame update
void Start()
    {
        AttachmentPointRadius = GetComponentInChildren<CircleCollider2D>().radius;
        if (EditButtonHolder)
        {
            EditButtonLeft = EditButtonHolder.transform.GetChild(0).gameObject;
            EditButtonRight = EditButtonHolder.transform.GetChild(1).gameObject;
        }
        InitializeAttachmentPoints();
    }
    // Update is called once per frame
    




    public void StartPickup()
    {
        isMoving = true;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.25f);
    }
    public void EndPickup()
    {
        isMoving = false;
        GetComponent<SpriteRenderer>().color = Color.white;
        if (isFirstTimePlace && treeExp > 0)
        {
            TreeLevelController.Instance.AddFurnitureExp(treeExp);
            isFirstTimePlace = false;
        }
    }

    

    public void SetScrollRotateIndexHolder(int _index) { scrollRotateIndexHolder = _index; }

    
    private void InitializeAttachmentPoints()
    {
        AttachmentPointsList.Clear();
        foreach (Transform _attachmentPoint in transform)
        {
            if (_attachmentPoint.gameObject.tag == "Decoration Attach Point" && _attachmentPoint.gameObject.activeInHierarchy)
            {
                AttachmentPointsList.Add(_attachmentPoint.localPosition);
                AttachmentPointRadius = _attachmentPoint.GetComponent<CircleCollider2D>().radius;
            }
        }
    }

    [ContextMenu("Initialize buttons")]
    private void InitializeButtons()
    {
        if (RemoveButton) RemoveButton.transform.position = GetComponent<BoxCollider2D>().bounds.max;
        if (EditButtonHolder)
        {
            EditButtonLeft = EditButtonHolder.transform.GetChild(0).gameObject;
            EditButtonRight = EditButtonHolder.transform.GetChild(1).gameObject;
            EditButtonHolder.transform.position = new Vector2(transform.position.x, GetComponent<BoxCollider2D>().bounds.min.y);
        }
        
        
    }

}
