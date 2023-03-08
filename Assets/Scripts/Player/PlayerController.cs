// Player Controller
// Moves the player using it's attached rigidbody
// Player can move left and right, Ground Jump, Air Jump, and Slide
// Ground Jumping supports coyote time
// Action inputs can be locked by subsribing the relevent functions to events

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    #region Events
    public event Action OnPlayerJump;
    public event Action OnPlayerGroundJump;
    public event Action OnPlayerAirJump;
    public event Action OnPlayerWallHop;
    public event Action OnPlayerWallJump;
    public event Action OnPlayerWallHit;
    public event Action OnPlayerWallSlide;
    public event Action OnPlayerWallSlideEnd;
    public event Action<GameObject> OnPlayerLand;
    public event Action OnPlayerGlide;
    public event Action OnPlayerGlideEnd;
    public event Action OnPlayerDash;
    #endregion

    #region Variables
    // State booleans
    public bool IsGrounded { get; private set; } = true;
    public bool IsDashing { get; private set; } = false;
    public bool IsTouchingWall { get; private set; } = false;
    public bool IsWallStuck { get; private set; } = false;
    public bool IsWallSliding { get; private set; } = false;
    public bool IsGliding { get; private set; } = false;
    public bool IsLockedInput { get; private set; } = false;
    public bool IsFacingRight { get; set; } = true;

    // Component References
    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;

    #region Movement
    [Header("Movement Settings")]

    [Tooltip("How fast the player moves on the ground")]
    [Range(5, 15)]
    [SerializeField] private float moveSpeed = 10f;

    [Tooltip("How fast the player accelerates in the air")]
    [Range(0, 2)]
    [SerializeField] private float airMoveAcceleration = 0.8f;

    [Tooltip("How fast the player decelerates when in air and no input")]
    [Range(0, 1)]
    [SerializeField] private float airDragMultiplier = 0.95f;

    private Vector2 movementInput = Vector2.zero;
    #endregion

    #region Jump
    [Space(10)]
    [Header("Jump Settings")]

    [Tooltip("Stores which layers should be used in the grounded check")]
    [SerializeField] private LayerMask platformLayerMask;

    [Tooltip("The jump force when the Player jumps while grounded")]
    [Range(0, 30)]
    [SerializeField] private float groundedJumpForce = 20f;

    [Tooltip("The jump force when the Player jumps while in the air (Not grounded)")]
    [Range(0, 30)]
    [SerializeField] private float airJumpForce = 14f;

    [Range(0, 1)]
    [Tooltip("Variable jump height strenth")]
    [SerializeField] private float variableJumpHegithMultiplier = 0.5f;

    [Tooltip("Stores how many times can the player air jump")]
    [Range(0, 3)]
    [SerializeField] private int maxAirJumps = 1;

    [Tooltip("Dictates the duration of coyoteTime")]
    [Range(0.05f, 0.3f)]
    [SerializeField] private float coyoteTime = 0.1f;

    // Tracks how many air jumps the Player has remaining
    private int remainingAirJumps = 1;
    // Used to determine if Player is in coyote time
    private float timeOfLastTrueIsGrounded = 0f;
    // Used to determine if coyote time should be ignored as the player ground jumped
    private float timeOfLastGroundedJump = 0f;
    // Tracks if the jump action input has been queued
    private bool jumpQueued = false;
    #endregion

    #region Wall
    [Space(10)]
    [Header("Wall Settings")]

    [Tooltip("Stores which layers should be used in the IsTouchingWall check")]
    [SerializeField] private LayerMask wallLayerMask;

    [Tooltip("The speed that the player slides down the wall")]
    [Range(0, 5)]
    [SerializeField] private float wallSlideSpeed = 2f;

    [Tooltip("The jump force when the Player jumps from the wall")]
    [Range(0, 2)]
    [SerializeField] private float wallCheckDistance = 1f;

    [Space(5)]
    [Tooltip("The jump force when the Player hops from the wall")]
    [Range(0, 30)]
    [SerializeField] private float wallHopForce = 10f;

    [Tooltip("The direction of the wall hop, Normalized during Initialisation")]
    [SerializeField] private Vector2 wallHopDirection = new Vector2(1, 0.5f);

    [Tooltip("The delay between wall hop input and the wall hop")]
    [Range(0, 0.5f)]
    [SerializeField] private float wallHopInputQueueTime = 0.1f;

    [Space(5)]
    [Tooltip("The jump force when the Player jumps from the wall")]
    [Range(0, 60)]
    [SerializeField] private float wallJumpForce = 20f;

    [Tooltip("The direction of the wall jump, Normalized during Initialisation")]
    [SerializeField] private Vector2 wallJumpDirection = new Vector2(1, 2);

    [Tooltip("The delay in seconds between the Player hitting the wall and beggining a wall slide")]
    [Range(0, 5)]
    [SerializeField] private float wallStickTime = 1.5f;

    [Tooltip("The delay between wall jumping and being able to move in the air")]
    [Range(0, 2)]
    [SerializeField] private float airMovementDisabledDelayAfterWallJump = 0.5f;

    private float timeOfLastWallHit = 0f;
    private float timeOfLastWallJump = 0f;
    private float timeOfWallHopInput = 0;
    private bool wallHopQueued = false;

    #endregion   

    #region Dash
    [Space(10)]
    [Header("Dash Settings")]
    [Tooltip("The speed of the dash")]
    [Range(0, 30)]
    [SerializeField] private float dashSpeed = 20f;

    [Tooltip("The duration the IsDashing state is true")]
    [Range(0, 2)]
    [SerializeField] private float dashDuration = 0.2f;

    [Tooltip("The cool down time between dashes")]
    [Range(0, 2)]
    [SerializeField] private float dashCoolDown = 1f;

    private readonly int maxDashes = 1;
    private int remainingDashes = 1;
    private float dashHeight = 0f;
    private float timeOfLastDash = 0f;
    private bool dashQueued = false;
    #endregion 

    #region Gliding
    [Header("Gliding Settings")]

    [Tooltip("How fast the player has to fall to trigger gliding")]
    [Range(0, 20)]
    [SerializeField] private float fallSpeedThatTriggersGliding = 10f;

    [Tooltip("How fast the player falls when gliding")]
    [Range(0, 20)]
    [SerializeField] private float glideFallSpeed = 4f;

    [Tooltip("How fast the player accelerates while gliding")]
    [Range(0, 2)]
    [SerializeField] private float glideAirMoveAcceleration = 0.8f;

    [Tooltip("How fast the player decelerates when gliding and no input")]
    [Range(0, 1)]
    [SerializeField] private float glideAirDragMultiplier = 0.95f;

    private List<AirCurrent> airCurrents = new List<AirCurrent>();
    #endregion

    #region Utility
    private Vector2 lastKnownGroundPosition;
    #endregion
    #endregion

    #region Functions
    #region Initialisation
    private void Awake()
    {
        // If there already exists an Instance of this singleton then destroy this object, else this is the singleton instance
        if (Instance != null) Destroy(gameObject);
        else Instance = this;

        // Reference local components
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();

        ResetActions();
    }

    private void Start()
    {
        DialogueController.Instance.OnConversationStart += LockPlayerInput;
        DialogueController.Instance.OnConversationEnd += UnLockPlayerInput;
    }
    #endregion

    #region Updates
    private void Update()
    {
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        CheckMovementDirection();
        CheckIfPlayerIsGrounded();
        CheckIfWallTouching();
        CheckIfWallStuck();
        CheckIfWallSliding();
        CheckIfDashing();

        HandleActionInput();
    }

    private void FixedUpdate()
    {
        if (IsDashing) DashingBehaviour();

        if (CanMove) ApplyMovement();
    }
    #endregion

    #region Movement
    private void ApplyMovement()
    {
        Vector2 velocity = rb.velocity;

        if (IsGrounded)
        {
            velocity = new Vector2(movementInput.x * moveSpeed, velocity.y);
        }
        // If NotGrounded and Not touching a wall
        else if (!IsTouchingWall)
        {
            if (velocity.y < -fallSpeedThatTriggersGliding) StartGliding();

            velocity = IsGliding ? GlidingMovement(velocity) : AirMovement(velocity);
        }
        // If wallSliding and the Player is falling faster than the wall slide speed, then set the Y velocity to wall slide speed
        else if (IsWallSliding && velocity.y < -wallSlideSpeed)
        {
            velocity.y = -wallSlideSpeed;
        }

        // Finally set the velocity
        rb.velocity = velocity;
    }

    private Vector2 AirMovement(Vector2 _velocity)
    {
        // If Movement Input
        if (movementInput.x != 0 && TimeSinceLastWallJump > airMovementDisabledDelayAfterWallJump)
        {
            // Add the air acceleration force
            _velocity.x += movementInput.x * airMoveAcceleration;

            // If we exceed the player move speed
            if (MathF.Abs(_velocity.x) > moveSpeed)
            {
                // Clamp the movespeed
                _velocity.x = moveSpeed * movementInput.x;
            }
        }
        // If no Movement Input
        else
        {
            // Slow the player down by the air drag
            _velocity.x *= airDragMultiplier;
        }

        return _velocity;
    }

    private Vector2 GlidingMovement(Vector2 _velocity)
    {
        _velocity.y = _velocity.y < -glideFallSpeed ? -glideFallSpeed : _velocity.y;

        foreach (AirCurrent airCurrent in airCurrents)
        {
            switch (airCurrent.AirCurrentDirectionType)
            {
                case AirCurrent.AirCurrentDirectionTypes.UP:
                    _velocity.y = _velocity.y < airCurrent.AirCurrentSpeed ? airCurrent.AirCurrentSpeed : _velocity.y;
                    break;

                case AirCurrent.AirCurrentDirectionTypes.LEFT:
                    if (movementInput.x > 0) _velocity.x = _velocity.x > airCurrent.AirCurrentFightSpeed ? airCurrent.AirCurrentFightSpeed : _velocity.x;
                    else _velocity.x = _velocity.x > -airCurrent.AirCurrentSpeed ? -airCurrent.AirCurrentSpeed : _velocity.x;

                    break;

                case AirCurrent.AirCurrentDirectionTypes.RIGHT:
                    if (movementInput.x < 0) _velocity.x = _velocity.x < -airCurrent.AirCurrentFightSpeed ? -airCurrent.AirCurrentFightSpeed : _velocity.x;
                    else _velocity.x = _velocity.x < airCurrent.AirCurrentSpeed ? airCurrent.AirCurrentSpeed : _velocity.x;
                    break;
            }
        }

        // If Movement Input
        if (movementInput.x != 0 && TimeSinceLastWallJump > airMovementDisabledDelayAfterWallJump)
        {
            // Add the air acceleration force
            _velocity.x += movementInput.x * glideAirMoveAcceleration;

            // If we exceed the player move speed
            if (MathF.Abs(_velocity.x) > moveSpeed)
            {
                // Clamp the movespeed
                _velocity.x = moveSpeed * movementInput.x;
            }
        }
        // If no Movement Input
        else
        {
            // Slow the player down by the air drag
            _velocity.x *= glideAirDragMultiplier;
        }

        return _velocity;
    }
    #endregion

    #region State Checks
    private void CheckMovementDirection()
    {
        if (!IsWallSliding && !IsWallStuck)
        {
            if (IsFacingRight && movementInput.x < 0)
            {
                IsFacingRight = false;
            }
            else if (!IsFacingRight && movementInput.x > 0)
            {
                IsFacingRight = true;
            }
        }
    }

    /// <summary>
    /// Uses a BoxCast on the platform layer to determine if the player is grounded, takes into consideration coyote time.
    /// </summary>
    /// <returns> Returns true if the player is either grounded or coyote grounded.</returns>
    private void CheckIfPlayerIsGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(capsuleCollider.bounds.center, new Vector3(capsuleCollider.bounds.size.x * 0.75f, capsuleCollider.bounds.size.y, capsuleCollider.bounds.size.z), 0f, Vector2.down, 0.001f, platformLayerMask);
        bool isGroundedThisFrame = raycastHit;
        DrawDebugInfo();

        // If the player is grounded this frame
        if (isGroundedThisFrame)
        {
            // And coyote time has passed since the last ground jump
            if (TimeSinceLastGroundedJump > coyoteTime)
            {
                // This is a true grounded
                timeOfLastTrueIsGrounded = Time.time;
                lastKnownGroundPosition = transform.position;

                // If the player was not grounded on the previous frame but is grounded this frame, then they have landed
                if (!IsGrounded)
                {
                    // Reset air jumps
                    ResetActions();

                    // If Player was wall sliding previouse frame, and landed this frame. flip the direction
                    if (IsWallSliding) IsFacingRight = !IsFacingRight;

                    if (IsGliding) StopGliding();
                    OnPlayerLand?.Invoke(raycastHit.collider.gameObject);
                }
            }
            // If the player is grounded this frame but coyote time has not passed since the last grounded jump
            // The player should not be grounded this frame
            // This prevents grounded being true the frame after a grounded jump
            else
            {
                isGroundedThisFrame = false;
            }
        }
        // If the player is not grounded this frame but is within coyote time and they didn't ground Jump, then they are grounded
        else if (TimeSinceLastTrueGround < coyoteTime && TimeSinceLastGroundedJump > coyoteTime)
        {
            isGroundedThisFrame = true;
        }

        IsGrounded = isGroundedThisFrame;
    }

    private void CheckIfWallTouching()
    {
        bool isFrontTouchingWall = Physics2D.Raycast(capsuleCollider.bounds.center, Vector2.right * FacingDirection, wallCheckDistance, wallLayerMask);
        bool isBackToughingWall = Physics2D.Raycast(capsuleCollider.bounds.center, Vector2.right * -FacingDirection, wallCheckDistance, wallLayerMask);

        bool isTouchingWallThisFrame = isFrontTouchingWall || isBackToughingWall;

        // If player is not on the ground and is touching wall this frame, but not the last frame, then the player has hit a wall
        if (!IsGrounded && isTouchingWallThisFrame && !IsTouchingWall)
        {
            if (IsGliding) StopGliding();

            if (isBackToughingWall) IsFacingRight = !IsFacingRight;

            timeOfLastWallHit = Time.time;
            WallStick();

            OnPlayerWallHit?.Invoke();
        }

        IsTouchingWall = isTouchingWallThisFrame;
    }

    private void CheckIfWallStuck()
    {
        // If IsWallStuck & wallStickTime is exceeded, then Unstick
        if (IsWallStuck)
        {
            if (TimeSinceLastWallHit > wallStickTime)
            {
                UnWallSick();

                if (!IsPlayerMoveingIntoWall && !wallHopQueued)
                {
                    wallHopQueued = true;
                    timeOfWallHopInput = Time.time;
                }
            }

            if (IsPlayerMoveingAwayFromWall && !wallHopQueued)
            {
                wallHopQueued = true;
                timeOfWallHopInput = Time.time;
            }
        }
    }

    private void CheckIfWallSliding()
    {
        // Player IsWallSliding if they are not grounded, touching a wall, not wall stuck, and they are falling
        bool IsWallSlidingThisFrame = !IsGrounded && IsTouchingWall && !IsWallStuck && rb.velocity.y <= 0 && IsPlayerMoveingIntoWall;

        if (IsWallSlidingThisFrame && !IsWallSliding) OnPlayerWallSlide?.Invoke();
        else if (!IsWallSlidingThisFrame && IsWallSliding) OnPlayerWallSlideEnd?.Invoke();

        IsWallSliding = IsWallSlidingThisFrame;
    }

    private void CheckIfDashing()
    {
        // If Player IsDashing & within dash duration they are still dashing, else they are not
        IsDashing = IsDashing && TimeSinceLastDash < dashDuration;
    }
    #endregion

    #region ActionInputs
    /// <summary>
    /// Executes the action inputs if they are available and the player input is not locked.
    /// </summary>
    private void HandleActionInput()
    {
        // If the dialogue system has locked the player's input
        if (IsLockedInput) return;

        ResolveQue();

        if (Input.GetButtonDown("Jump") || jumpQueued)
        {
            if (CanJump) Jump();
            else jumpQueued = true;
        }
        else if (!IsGrounded && !IsTouchingWall && Input.GetButtonUp("Jump"))
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * variableJumpHegithMultiplier);
        }

        if (Input.GetButtonDown("Dash") || dashQueued)
        {
            if (CanDash) Dash();
            else dashQueued = true;
        }

        if (Input.GetButton("Drop"))
        {
            if (IsGliding) StopGliding();
            if (IsWallStuck) UnWallSick();
        }

    }

    /// <summary>
    /// Execute the queued action input if the action becomes available.
    /// </summary>
    void ResolveQue()
    {
        // If you're not holding the button after quing an action, assume you don't wanna do it
        if (jumpQueued && !Input.GetButton("Jump")) jumpQueued = false;

        if (dashQueued && !Input.GetButton("Dash")) dashQueued = false;

        if(wallHopQueued)
        {
            if(!IsTouchingWall)
            {
                wallHopQueued = false;
            }
            else if (TimeSinceWallHopInput > wallHopInputQueueTime)
            {
                wallHopQueued = false;
                WallHop();
            }
        }
    }
    #endregion

    #region Actions
    /// <summary>
    /// Makes the player jump, if they're grounded it's a grounded jump, else it's an air jump.
    /// </summary>
    private void Jump()
    {
        jumpQueued = false;
        if (IsWallStuck) UnWallSick();
        if (IsGliding) StopGliding();

        // If player is grounded do a ground jump
        if (IsGrounded)
        {
            IsGrounded = false;

            rb.velocity = new Vector2(rb.velocity.x, groundedJumpForce);
            timeOfLastGroundedJump = Time.time;

            OnPlayerGroundJump?.Invoke();
        }
        else if (IsTouchingWall)
        {
            IsFacingRight = !IsFacingRight;

            rb.AddForce(new Vector2(FacingDirection * wallJumpForce * wallJumpDirection.x, wallJumpForce * wallJumpDirection.y), ForceMode2D.Impulse);
            timeOfLastWallJump = Time.time;
            wallHopQueued = false;

            OnPlayerWallJump?.Invoke();
        }

        // Else it's an air jump
        else
        {
            remainingAirJumps--;
            rb.velocity = new Vector2(rb.velocity.x, airJumpForce);

            OnPlayerAirJump?.Invoke();
        }

        OnPlayerJump?.Invoke();
    }

    private void WallHop()
    {
        if (IsWallStuck) UnWallSick();

        IsFacingRight = !IsFacingRight;
        rb.AddForce(new Vector2(FacingDirection * wallHopForce * wallHopDirection.x, wallHopForce * wallHopDirection.y), ForceMode2D.Impulse);

        OnPlayerWallHop?.Invoke();
    }

    private void DashingBehaviour()
    {
        if (transform.position.y < dashHeight)
        {
            transform.position = new Vector2(transform.position.x, dashHeight);
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }

    private void Dash()
    {
        if (IsGliding) StopGliding();

        IsDashing = true;
        timeOfLastDash = Time.time;

        // If its an air dash
        if (!IsGrounded) remainingDashes--;

        dashHeight = transform.position.y;

        rb.velocity = new Vector2(FacingDirection * dashSpeed, rb.velocity.y);
        OnPlayerDash?.Invoke();
    }
    #endregion

    #region Gliding
    private void StartGliding()
    {
        // If holding the drop key then do not start gliding
        if (Input.GetButton("Drop") || IsGliding) return;

        IsGliding = true;
        OnPlayerGlide?.Invoke();
    }

    private void StopGliding()
    {
        IsGliding = false;
        OnPlayerGlideEnd?.Invoke();
    }
    #endregion

    #region WallStick
    private void WallStick()
    {
        IsWallStuck = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;

        // Reset air jumps
        remainingAirJumps = maxAirJumps;
    }

    private void UnWallSick()
    {
        IsWallStuck = false;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.velocity = new Vector2(0, -.1f);
    }
    #endregion

    #region Utilities
    private void ResetActions()
    {
        remainingAirJumps = maxAirJumps;
        remainingDashes = maxDashes;
    }

    public void ResetPlayerToLastKnownPosition()
    {
        transform.position = lastKnownGroundPosition;
    }

    /// <summary>
    /// Locks the Players movement and action inputs.
    /// </summary>
    /// <param name="conversation">Parameter not used, is required to subsribe to the event</param>
    private void LockPlayerInput(/*Conversation conversation = null*/)
    {
        IsLockedInput = true;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    /// <summary>
    /// Un-Locks the players movement and actions inputs.
    /// </summary>
    /// <param name="conversation">Parameter not used, is required to subsribe to the event</param>
    private void UnLockPlayerInput(/*Conversation conversation = null*/)
    {
        IsLockedInput = false;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    /// <summary>
    /// Returns true if the Player is able to Move, Such as if the play is not sliding or dashing.
    /// </summary>
    private bool CanMove { get { return !IsDashing && !IsLockedInput; } }

    /// <summary>
    /// Returns true if the Player is able to Jump.
    /// </summary>
    private bool CanJump { get { return IsGrounded || IsTouchingWall || CanAirJump; } }

    /// <summary>
    /// Returns true if the Player is able to air Jump
    /// </summary>
    private bool CanAirJump { get { return !IsGrounded && !IsTouchingWall && remainingAirJumps > 0; } }

    /// <summary>
    /// Returns true if the Player is able to Dash.
    /// </summary>
    private bool CanDash { get { return !IsDashing && !IsWallStuck && (remainingDashes > 0 || IsGrounded) && TimeSinceLastDash > dashCoolDown; } }

    private int FacingDirection { get { return IsFacingRight ? 1 : -1; } }
    private float TimeSinceLastDash { get { return Time.time - timeOfLastDash; } }
    private float TimeSinceLastTrueGround { get { return Time.time - timeOfLastTrueIsGrounded; } }
    private float TimeSinceLastGroundedJump { get { return Time.time - timeOfLastGroundedJump; } }
    private float TimeSinceLastWallHit { get { return Time.time - timeOfLastWallHit; } }
    private float TimeSinceLastWallJump { get { return Time.time - timeOfLastWallJump; } }
    private float TimeSinceWallHopInput { get { return Time.time - timeOfWallHopInput; } }

    private bool IsPlayerMoveingIntoWall { get { return IsTouchingWall && (IsFacingRight && movementInput.x > 0) || (!IsFacingRight && movementInput.x < 0); } }
    private bool IsPlayerMoveingAwayFromWall { get { return IsTouchingWall && (IsFacingRight && movementInput.x < 0) || (!IsFacingRight && movementInput.x > 0); } }

    public void AddAirCurrent(AirCurrent _airCurrent)
    {
        if (!airCurrents.Contains(_airCurrent))
        {
            airCurrents.Add(_airCurrent);
            StartGliding();
        }
    }

    public void RemoveAirCurrent(AirCurrent _airCurrent)
    {
        if (airCurrents.Contains(_airCurrent)) airCurrents.Remove(_airCurrent);       
    }
    #endregion

    #region Collision
    #endregion

    #region Debug
    /// <summary>
    /// Draws Jump reset debug info.
    /// </summary>
    private void DrawDebugInfo()
    {
#if UNITY_EDITOR
        Color rayColour = IsGrounded ? Color.green : Color.red;
        Debug.DrawRay(capsuleCollider.bounds.center - new Vector3(capsuleCollider.bounds.extents.x, capsuleCollider.bounds.extents.y + 0.1f), Vector2.right * (capsuleCollider.bounds.extents.x * 2), rayColour);

        rayColour = IsTouchingWall ? Color.green : Color.red;
        Debug.DrawRay(capsuleCollider.bounds.center, wallCheckDistance * FacingDirection * Vector2.right, rayColour);
#endif
    }
    #endregion
    #endregion
}