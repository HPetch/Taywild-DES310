using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class DecorationObject : MonoBehaviour
{
    public enum ObjectType {DECORATION_OBJECT, ENVIROMENT_OBJECT, TRASH_OBJECT} //Decoration object - Move Edit Delete, Enviroment - Edit, Trash - Delete
    public ObjectType objectType;
    [field: SerializeField] public List<Vector3> AttachmentPointsList{ get; private set; }
    public float AttachmentPointRadius { get; private set; }

    private void Start()
    {
        
    }

    public void StartPickup()
    {
        GetComponent<SpriteRenderer>().color = Color.gray;
    }
    public void EndPickup()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    private void Reset()
    {
        foreach (Transform _attachmentPoint in transform)
        {
            
            if (_attachmentPoint.gameObject.tag == "Decoration Attach Point")
            {
                AttachmentPointsList.Add(_attachmentPoint.position);
                AttachmentPointRadius = _attachmentPoint.GetComponent<CircleCollider2D>().radius;
            }
        }
    }
}
