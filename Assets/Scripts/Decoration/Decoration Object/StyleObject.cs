using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StyleObject : DecorationObject
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    [ContextMenu("Initialize buttons")]
    private void InitializeButtons()
    {
        if (!RemoveButton) Instantiate(pickupButtonPrefab);
        if (!EditButtonHolder)
        {
            Instantiate(editButtonHolderPrefab);
            EditButtonLeft = EditButtonHolder.transform.GetChild(0).gameObject;
            EditButtonRight = EditButtonHolder.transform.GetChild(1).gameObject;
            EditButtonHolder.transform.position = new Vector2(transform.position.x, GetComponent<BoxCollider2D>().bounds.min.y);
        }
        RemoveButton.transform.position = GetComponent<BoxCollider2D>().bounds.max;

    }
}
