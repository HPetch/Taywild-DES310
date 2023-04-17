using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceBroadcaster : MonoBehaviour
{
    //The Player Effect Controller will fire a Raycast2D to check for colliders when player lands on the ground
    //It will access this script if it finds it, and use a method that returns the current SURFACE_TYPE

    [SerializeField] private PlayerEffectController.SURFACE_TYPE surfaceType;

    public PlayerEffectController.SURFACE_TYPE getSurfaceType()
    {
        return surfaceType;
    }

}
