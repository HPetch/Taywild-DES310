using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [SerializeField] private Vector2 parallaxEffectMultiplier = new Vector2(1.0f, 0.5f);
    [SerializeField] private bool infiniteScrolling = true;

    private Transform playerCameraTransform = null;
    private Vector3 lastCameraPosition = Vector2.one;
    private float textureUnitSizeX;

    private void Start()
    {
        GetComponent<SpriteRenderer>().enabled = true;

        playerCameraTransform = CameraController.Instance.transform;
        lastCameraPosition = playerCameraTransform.position;

        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        textureUnitSizeX = (sprite.texture.width / sprite.pixelsPerUnit) * 2;
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = (playerCameraTransform.position - lastCameraPosition) * parallaxEffectMultiplier;
        transform.position += deltaMovement;
        lastCameraPosition = playerCameraTransform.position;

        if (infiniteScrolling)
        {
            if (Mathf.Abs(playerCameraTransform.position.x - transform.position.x) >= textureUnitSizeX)
            {
                float offsetPositionX = (playerCameraTransform.position.x - transform.position.x) % textureUnitSizeX;
                offsetPositionX *= playerCameraTransform.position.x - transform.position.x < 0 ? -1 : 1;
                transform.position += new Vector3(textureUnitSizeX + offsetPositionX, 0f, 0f);
            }
        }
    }
}