using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureObject : DecorationObject
{

    [field: SerializeField] public List<Vector3> AttachmentPointsList { get; private set; }
    public float AttachmentPointRadius { get; private set; }
    [field: SerializeField] public int scrollRotateIndexHolder { get; private set; }

    








    // Start is called before the first frame update
    void Start()
    {
        AttachmentPointRadius = GetComponentInChildren<CircleCollider2D>().radius;
        if (PickupButton) PickupButton.transform.position = GetComponent<BoxCollider2D>().bounds.max;
    }

    // Update is called once per frame
    void Update()
    {

    }




    public void StartPickup()
    {
        isMoving = true;
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.25f);
    }
    public void EndPickup()
    {
        isMoving = false;
        GetComponent<SpriteRenderer>().color = Color.white;
    }


    public void SetScrollRotateIndexHolder(int _index) { scrollRotateIndexHolder = _index; }

    [ContextMenu("Initialize attachment points")]
    private void InitializeAttachmentPoints()
    {
        
        foreach (Transform _attachmentPoint in transform)
        {
            if (_attachmentPoint.gameObject.tag == "Decoration Attach Point")
            {
                AttachmentPointsList.Add(_attachmentPoint.localPosition);
                AttachmentPointRadius = _attachmentPoint.GetComponent<CircleCollider2D>().radius;
            }
        }
    }

    [ContextMenu("Initialize buttons")]
    private void InitializeButtons()
    {
        if (!PickupButton) PickupButton = Instantiate(pickupButtonPrefab, transform);
        if (!EditButtonHolder)
        {
            Instantiate(editButtonHolderPrefab, transform);
            foreach(DecorationButton _button in GetComponentsInChildren<DecorationButton>())
            {
                if (_button.buttonType == DecorationButton.ButtonType.STYLE_HOLDER) EditButtonHolder = _button.gameObject;
            }
            EditButtonLeft = EditButtonHolder.transform.GetChild(0).gameObject;
            EditButtonRight = EditButtonHolder.transform.GetChild(1).gameObject;
            EditButtonHolder.transform.position = new Vector2(transform.position.x, GetComponent<BoxCollider2D>().bounds.min.y);
        }
        PickupButton.transform.position = GetComponent<BoxCollider2D>().bounds.max;
        
    }

}
