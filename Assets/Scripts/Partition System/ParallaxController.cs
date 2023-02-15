using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [SerializeField] private Vector2 parallaxEffectMultiplier = new Vector2(1.0f, 0.5f);

    private Transform playerCameraTransform = null;
    private Vector3 lastCameraPosition = Vector2.one;

    private void Start()
    {
        playerCameraTransform = CameraController.Instance.transform;
        lastCameraPosition = playerCameraTransform.position;
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = (playerCameraTransform.position - lastCameraPosition) * parallaxEffectMultiplier;
        transform.position += deltaMovement;
        lastCameraPosition = playerCameraTransform.position;
    }
}