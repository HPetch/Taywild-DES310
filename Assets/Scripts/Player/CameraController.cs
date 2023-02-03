using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    #region Variables
    private Camera cameraComponent;
    private Transform player;

    [SerializeField] private float smoothSpeed = 0.12f;
    [SerializeField] private Vector2 screenLimitX = new Vector2();
    [SerializeField] private Vector2 screenLimitY = new Vector2();
    [SerializeField] private Vector3 playerOffset = new Vector2(0, 1);

    // Shaking Variables
    private bool isShaking = false;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

    // Zoom
    [SerializeField] private float zoomSpeed = 10f;
    private float defaultSize = 5;
    private float targetSize = 5;
    #endregion

    private void Awake()
    {
        Instance = this;
        cameraComponent = GetComponent<Camera>();
    }


    private void Start()
    {
        player = PlayerController.Instance.transform;

        defaultSize = cameraComponent.orthographicSize;
        targetSize = defaultSize;
        playerOffset.z = transform.position.z;
    }

    private void FixedUpdate()
    {
        // Get target position
        Vector3 targetPosition = player.position + playerOffset;
        // Interpolate towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);

        if (isShaking)
        {
            // Get random direction multiplied by the magnitude
            Vector2 shake = Random.insideUnitCircle.normalized * shakeMagnitude;
            transform.position += new Vector3(shake.x, shake.y, 0);

            // Decrement the duration
            shakeDuration -= Time.deltaTime;
            if (shakeDuration <= 0f) isShaking = false;
        }

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, screenLimitX.x, screenLimitX.y), Mathf.Clamp(transform.position.y, screenLimitY.x, screenLimitY.y), transform.position.z);

        if (cameraComponent.orthographicSize != targetSize)
        {
            cameraComponent.orthographicSize = Mathf.Lerp(cameraComponent.orthographicSize, targetSize, Time.fixedDeltaTime * zoomSpeed);
        }
    }

    public void Shake(float _duration, float _magnitude)
    {
        isShaking = true;
        shakeDuration = _duration > shakeDuration ? _duration : shakeDuration;
        shakeMagnitude = _magnitude > shakeMagnitude ? _magnitude : shakeMagnitude;
    }

    public void Zoom(float _targetSize = 0)
    {
        targetSize = _targetSize == 0 ? defaultSize : _targetSize;
    }
}
