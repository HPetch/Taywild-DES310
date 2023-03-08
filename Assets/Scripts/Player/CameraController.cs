// Camera Controller
// Can be Lock or UnLocked
// Locked camera will lerp to the center of the current partition
// Unlocked camera will follow the player position

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
    [SerializeField] private Vector2 dialogueOffset = new Vector3(0, 3);
    private Vector2 offset;
    private Transform targetTransform;

    // Shaking Variables
    private bool isShaking = false;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

    // Zoom Variables
    [SerializeField] private float zoomSpeed = 10f;
    private float defaultSize = 5;
    private float targetZoomSize = 5;
    private float partitionTargetZoomSize = 5;

    // Punch In
    public bool IsPunched { get; set; } = false;

    private float punchSpeed = 20f;
    private float targetPunchSize = 5f;
    private bool holdPunch = false;
    #endregion

    #region Methods
    #region Initialisation
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
        DialogueController.Instance.OnConversationStart += OnConversationStarted;
        DialogueController.Instance.OnConversationEnd += OnConversationEnded;

        // Reference the player once on start as oppsed to each frame
        playerTransform = PlayerController.Instance.transform;
        // Set inital target transform to player
        targetTransform = playerTransform;

        // Set default varialbe
        defaultSize = CameraSize;
        targetZoomSize = defaultSize;
        offset = playerOffset;
    }
    #endregion

    private void Update()
    {
        CheckIfPunched();
    }

    // Camera functionality in FixedUpdate to ensure up to date player physics
    private void FixedUpdate()
    {
        // Punch
        if (IsPunched) CameraSize = Mathf.Lerp(CameraSize, targetPunchSize, Time.fixedDeltaTime * punchSpeed);
        // Zoom
        else CameraSize = Mathf.Lerp(CameraSize, targetZoomSize, Time.fixedDeltaTime * zoomSpeed);

        // Get target position
        Vector2 targetPosition = (Vector2)targetTransform.position + offset;
        targetPosition = ClampCameraToPartition(targetPosition);

        Vector2 newPosition = transform.position;

        if (DecorationController.Instance.isEditMode)
        {
            // Only move camera in edit mode while holding control
            if (Input.GetKey(KeyCode.LeftControl))
            {
                targetPosition = Vector2.Lerp(playerTransform.position, targetPosition, 0.5f);
                targetPosition = ClampCameraToPartition(targetPosition);
                newPosition = Vector2.Lerp(transform.position, new Vector2(targetPosition.x, targetPosition.y), Time.fixedDeltaTime * cameraSpeed);
            }
        }
        // Move towards the target position
        else
        {
            newPosition = Vector2.Lerp(transform.position, new Vector2(targetPosition.x, targetPosition.y), Time.fixedDeltaTime * cameraSpeed);
        }

        transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);


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
        targetZoomSize = _targetSize;
    }

    public void SetTarget(Transform _transform) 
    {
        targetTransform = _transform;
    }

    public void ResetTarget()
    {
        targetTransform = playerTransform;
    }

    public void ZoomToDefault()
    {
        Zoom(defaultSize);
    }

    private void PartitionTransition(Partition partition)
    {
        partitionTargetZoomSize = partition.TargetCameraSize;
        Zoom(partitionTargetZoomSize);
    }

    private void OnConversationStarted()
    {
        Vector2 midPoint = Vector2.Lerp(playerTransform.position, DialogueController.Instance.Character.transform.position, 0.5f);
        dialogueOffset.x = midPoint.x - playerTransform.position.x;
        offset = dialogueOffset;
        Zoom(5);
    }

    private void OnConversationEnded()
    {
        offset = playerOffset;
        Zoom(partitionTargetZoomSize);
    }

    

    public void PunchIn(float strength, float speed, bool _holdPunch = false)
    {
        IsPunched = true;
        punchSpeed = speed;
        holdPunch = _holdPunch;
        targetPunchSize = targetZoomSize - strength;
    }

    public void PunchOut(float strength, float speed, bool _holdPunch = false)
    {
        PunchIn(-strength, speed, _holdPunch);
    }

    private void CheckIfPunched()
    {
        if (IsPunched && !holdPunch && Mathf.Abs(CameraSize - targetPunchSize) < 0.05f) IsPunched = false;
    }

    #region Utility
    public float CameraSize { get { return cameraComponent.orthographicSize; } set { cameraComponent.orthographicSize = value; } }

    public Vector2 MouseWorldPosition { get { return cameraComponent.ScreenToWorldPoint(Input.mousePosition); } }

    private Vector2 ClampCameraToPartition(Vector2 position)
    {
        Rect partitionRect = TransitionController.Instance.CurrentPartition.PartitionRect;
        Vector2 partitionPosition = TransitionController.Instance.CurrentPartition.transform.position;

        // Limit
        float height = CameraSize * 2.0f;
        float width = height * Screen.width / Screen.height;
        float limitX = (partitionRect.width - width) * 0.5f;
        float limitY = (partitionRect.height - height) * 0.5f;

        return new Vector2(Mathf.Clamp(position.x, partitionPosition.x - limitX, partitionPosition.x + limitX), Mathf.Clamp(position.y, partitionPosition.y - limitY, partitionPosition.y + limitY));
    }
    #endregion
    #endregion
}