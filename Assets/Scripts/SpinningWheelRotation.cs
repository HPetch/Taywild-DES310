using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinningWheelRotation : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f; //Rate at which this will rotate per FixedUpdate
    void FixedUpdate()
    {
        //Add rotation via euler angles
        Vector3 eulerRotation = transform.rotation.eulerAngles + new Vector3(0f,0f,rotationSpeed);

        //Apply rotation converted to quarternion to transform.
        transform.rotation = Quaternion.Euler(eulerRotation);

    }
}
