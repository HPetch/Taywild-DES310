using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class DecorationObject : MonoBehaviour
{
    public enum ObjectType {DECORATION_OBJECT, ENVIROMENT_OBJECT, TRASH_OBJECT} //Decoration object - Move Edit Delete, Enviroment - Edit, Trash - Delete
    public ObjectType objectType;

    public void StartPickup()
    {
        GetComponent<SpriteRenderer>().color = Color.gray;
    }
    public void EndPickup()
    {
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}
