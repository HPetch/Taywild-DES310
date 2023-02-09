// Camera Controller
// Can be Lock or UnLocked
// Locked camera will lerp to the center of the current partition
// Unlocked camera will follow the player position

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    #region Variables
    // Component References
    private Camera cameraComponent;
    private Transform playerTransform;

    // Tracking Variables
    [SerializeField] private float cameraSpeed = 2;
    [SerializeField] private Vector2 playerOffset = new Vector3(0, 1);
    public bool IsLocked { get; private set; } = true;
    private Vector2 targetPosition = Vector2.zero;

    // Shaking Variables
    private bool isShaking = false;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

    // Zoom Variables
    [SerializeField] private float zoomSpeed = 10f;
    private float defaultSize = 5;
    private float targetSize = 5;
    #endregion

    private void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        // Refernce local component
        cameraComponent = GetComponent<Camera>();
    }

    private void Start()
    {
        // Subsribe the 'PartitionTransition' to the OnTransitionStart Event
        TransitionController.Instance.OnTransitionStart += PartitionTransition;

        // Reference the player once on start as oppsed to each frame
        playerTransform = PlayerController.Instance.transform;

        // Set default varialbe
        defaultSize = cameraComponent.orthographicSize;
        targetSize = defaultSize;
    }

    // Camera functionality in FixedUpdate to ensure up to date player physics
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
        //transform.position = new Vector3(Mathf.Clamp(transform.position.x, screenLimitX.x, screenLimitX.y), Mathf.Clamp(transform.position.y, screenLimitY.x, screenLimitY.y), transform.position.z);

        // Zoom
        if (cameraComponent.orthographicSize != targetSize)
        {
            cameraComponent.orthographicSize = Mathf.Lerp(cameraComponent.orthographicSize, targetSize, Time.fixedDeltaTime * zoomSpeed);
        }
    }

    /// <summary>
    /// Shakes the camera by magnitude for a duration, greater variable size takes precedence
    /// </summary>
    public void Shake(float _duration, float _magnitude)
    {
        isShaking = true;
        shakeDuration = _duration > shakeDuration ? _duration : shakeDuration;
        shakeMagnitude = _magnitude > shakeMagnitude ? _magnitude : shakeMagnitude;
    }

    /// <summary>
    /// Sets the target ortho size of the player camera
    /// </summary>
    public void Zoom(float _targetSize = 0)
    {
        targetSize = _targetSize == 0 ? defaultSize : _targetSize;
    }

    /// <summary>
    /// Lock the camera onto a static target posiition
    /// </summary>
    /// <param name="_targetPosition"> Target position of the camera focus point </param>
    private void LockCamera(Vector2 _targetPosition)
    {
        IsLocked = true;
        targetPosition = _targetPosition;
    }

    /// <summary>
    /// Unlock the camera
    /// </summary>
    private void UnLockCamera()
    {
        IsLocked = false;
    }

    /// <summary>
    /// Function subsribed to OnTransitionStart, sets the camera control varialbes based on partition settings
    /// </summary>
    private void PartitionTransition(Partition partition)
    {
        if (partition.IsCameraFixed)
        {
            // If camera is locked, center the camera on the partition
            LockCamera(partition.transform.position);
        }
        else
        {
            // Follow the player
            UnLockCamera();            
        }

        Zoom(partition.TargetCameraSize);
    }
}
