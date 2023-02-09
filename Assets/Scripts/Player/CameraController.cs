using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    #region Variables
    private Camera cameraComponent;
    private Transform playerTransform;

    [Space(20)]
    [SerializeField] private float cameraSpeed = 2;
    [SerializeField] private Vector2 screenLimitX = new Vector2();
    [SerializeField] private Vector2 screenLimitY = new Vector2();
    [SerializeField] private Vector2 playerOffset = new Vector3(0, 1);

    public bool IsLocked { get; private set; } = true;
    private Vector2 targetPosition = Vector2.zero;

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
        playerTransform = PlayerController.Instance.transform;

        defaultSize = cameraComponent.orthographicSize;
        targetSize = defaultSize;
    }

    private void FixedUpdate()
    {
        // Get target position
        targetPosition = !IsLocked ? (Vector2)playerTransform.position + playerOffset : targetPosition;

        // Move towards the target position
        transform.position = Vector3.Lerp(transform.position, new Vector3(targetPosition.x, targetPosition.y, transform.position.z), Time.fixedDeltaTime * cameraSpeed);


        // Shake
        if (isShaking)
        {
            // Get random direction multiplied by the magnitude
            Vector2 shake = Random.insideUnitCircle.normalized * shakeMagnitude;
            transform.position += new Vector3(shake.x, shake.y, 0);

            // Decrement the duration
            shakeDuration -= Time.fixedDeltaTime;
            if (shakeDuration <= 0f) isShaking = false;
        }

        // Clamp
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, screenLimitX.x, screenLimitX.y), Mathf.Clamp(transform.position.y, screenLimitY.x, screenLimitY.y), transform.position.z);

        // Zoom
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

    public void LockCamera(Vector2 _targetPosition)
    {
        IsLocked = true;
        targetPosition = _targetPosition;
    }

    public void UnLockCamera()
    {
        IsLocked = false;
    }
}
