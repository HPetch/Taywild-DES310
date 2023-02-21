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
    public event Action OnPlayerLand;
    public event Action OnPlayerSlide;
    public event Action OnPlayerDash;
    public event Action<GrapplePoint> OnPlayerGrapple;
    public event Action OnPlayerGrappleEnd;
    #endregion

    #region Variables
    // State booleans
    public bool IsGrounded { get; private set; } = true;
    public bool IsSliding { get; private set; } = false;
    public bool IsDashing { get; private set; } = false;
    public bool IsTouchingWall { get; private set; } = false;
    public bool IsWallStuck { get; private set; } = false;
    public bool IsWallSliding { get; private set; } = false;
    public bool IsGliding { get; private set; } = false;
    public bool IsGrappling { get; set; } = false;
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
    [Range(0, 20)]
    [SerializeField] private float airMoveAcceleration = 2f;

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

    [Space(5)]
    [Tooltip("The jump force when the Player jumps from the wall")]
    [Range(0, 30)]
    [SerializeField] private float wallJumpForce = 20f;

    [Tooltip("The direction of the wall jump, Normalized during Initialisation")]
    [SerializeField] private Vector2 wallJumpDirection = new Vector2(1, 2);

    [Tooltip("The delay in seconds between the Player hitting the wall and beggining a wall slide")]
    [Range(0, 1)]
    [SerializeField] private float wallStickTime = 0.3f;

    private float timeOfLastWallHit = 0f;

    #endregion

    #region Slide
    [Space(10)]
    [Header("Slide Settings")]

    [Tooltip("The initial strength of the slide")]
    [Range(0, 25)]
    [SerializeField] private float slideForce = 15f;

    [Tooltip("The duration of the slide")]
    [Range(0, 2)]
    [SerializeField] private float slideDuration = 0.4f;

    [Tooltip("The cooldown between slides")]
    [Range(0, 2)]
    [SerializeField] private float slideCooldown = 1f;

    [Space(5)]
    [Tooltip("Dictates the behaviour of the slide over it's duration")]
    [SerializeField] private AnimationCurve slideCurve;

    // Tracks if the slide action input has been queued
    private bool slideQueued = false;
    // Tracks how long the slide has lasted
    private float slideTimeElapsed = 0f;
    // Used to sample the slide curve
    private float slideCurveSamplePoint = 0f;
    private float timeOfLastSlide = 0f;
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

    #region Grapple
    [Header("Grapple Settings")]

    [Tooltip("The jump force when the player is grappling")]
    [Range(0, 30)]
    [SerializeField] private float grapplingJumpForce = 15f;

    [Tooltip("The max speed of the player when grappling")]
    [Range(0, 30)]
    [SerializeField] private float grappleMoveSpeed = 15f;

    [Tooltip("How fast the player accelerates when grappling")]
    [Range(0, 10)]
    [SerializeField] private float grappleAcceleration = 2f;

    private List<GrapplePoint> grapplePoints = new List<GrapplePoint>();
    private DistanceJoint2D grappleJoint;
    private bool grappleQueued = false;
    private GrapplePoint grapplePoint;
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
        grappleJoint = GetComponent<DistanceJoint2D>();
        grappleJoint.enabled = false;

        wallHopDirection.Normalize();
        wallJumpDirection.Normalize();

        ResetActions();
    }

    private void Start()
    {

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
        if (IsSliding) SlidingBehaviour();
        if (IsDashing) DashingBehaviour();

        if (CanMove) ApplyMovement();      
    }
    #endregion

    private void ApplyMovement()
    {
        Vector2 velocity = rb.velocity;

        if (IsGrounded)
        {
            velocity = new Vector2(movementInput.x * moveSpeed, velocity.y);
        }
        else if (IsGrappling)
        {
            // If the graple point is not valid then clamp the velocity
            if (!grapplePoint.IsGrappleValid && velocity.y > 0f)
            {
                velocity *= new Vector2(0.8f, 0.8f);
            }

            // If Movement Input
            else if (movementInput.x != 0)
            {
                velocity.x += movementInput.x * grappleAcceleration;

                // Clamp the movespeed
                velocity = Vector2.ClampMagnitude(velocity, grappleMoveSpeed);
            }

        }
        // If NotGrounded and Not touching a wall and not Grappling
        else if (!IsTouchingWall)
        {
            // If Movement Input
            if (movementInput.x != 0)
            {
                // Add the air acceleration force
                velocity.x += movementInput.x * airMoveAcceleration;

                // If we exceed the player move speed
                if (MathF.Abs(velocity.x) > moveSpeed)
                {
                    // Clamp the movespeed
                    velocity.x = moveSpeed * movementInput.x;
                }
            }
            // If no Movement Input
            else
            {
                // Slow the player down by the air drag
                velocity.x *= airDragMultiplier;
            }
        }

        // If wallSliding and the Player is falling faster than the wall slide speed, then set the Y velocity to wall slide speed
        if (IsWallSliding && velocity.y < -wallSlideSpeed)
        {
            velocity.y = -wallSlideSpeed;
        }

        // Finally set the velocity
        rb.velocity = velocity;
    }

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
        bool isGroundedThisFrame = Physics2D.BoxCast(capsuleCollider.bounds.center, new Vector3(capsuleCollider.bounds.size.x * 0.75f, capsuleCollider.bounds.size.y, capsuleCollider.bounds.size.z), 0f, Vector2.down, 0.001f, platformLayerMask);
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

                    OnPlayerLand?.Invoke();
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
        bool isTouchingWallThisFrame = Physics2D.Raycast(capsuleCollider.bounds.center, Vector2.right * FacingDirection, wallCheckDistance, wallLayerMask);

        // If player is not on the ground and is touching wall this frame, but not the last frame, then the player has hit a wall
        if (!IsGrounded && isTouchingWallThisFrame && !IsTouchingWall)
        {
            timeOfLastWallHit = Time.time;
            WallStick();

            OnPlayerWallHit?.Invoke();
        }

        IsTouchingWall = isTouchingWallThisFrame;
    }

    private void CheckIfWallStuck()
    {
        // If IsWallStuck & wallStickTime is exceeded, then Unstick
        if (IsWallStuck && TimeSinceLastWallHit > wallStickTime)
        {
            UnWallSick();
        }
    }

    private void CheckIfWallSliding()
    {
        // Player IsWallSliding if they are not grounded, touching a wall, not wall stuck, and they are falling
        bool IsWallSlidingThisFrame = !IsGrounded && IsTouchingWall && !IsWallStuck && rb.velocity.y <= 0;

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

        if (Input.GetButtonDown("Slide") || slideQueued)
        {
            if (CanSlide) Slide();
            else slideQueued = true;
        }

        if (Input.GetButtonDown("Dash") || dashQueued)
        {
            if (CanDash) Dash();
            else dashQueued = true;
        }

        if (Input.GetButtonDown("Grapple") || grappleQueued)
        {
            if (CanGrapple) Grapple();
            else grappleQueued = true;
        }
    }

    /// <summary>
    /// Execute the queued action input if the action becomes available.
    /// </summary>
    void ResolveQue()
    {
        // If you're not holding the button after quing an action, assume you don't wanna do it
        if (jumpQueued && !Input.GetButton("Jump")) jumpQueued = false;

        if (slideQueued && !Input.GetButton("Slide")) slideQueued = false;

        if (dashQueued && !Input.GetButton("Dash")) dashQueued = false;

        if (grappleQueued && !Input.GetButton("Grapple")) grappleQueued = false;
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

        if (IsGrappling)
        {
            IsGrappling = false;
            grapplePoint = null;
            grappleJoint.enabled = false;

            rb.velocity = new Vector2(rb.velocity.x, grapplingJumpForce);

            OnPlayerGrappleEnd?.Invoke();
        }        

        // If player is grounded do a ground jump
        else if (IsGrounded)
        {
            IsGrounded = false;

            rb.velocity = new Vector2(rb.velocity.x, groundedJumpForce);
            timeOfLastGroundedJump = Time.time;

            OnPlayerGroundJump?.Invoke();
        }

        // If the player is not grounded, is wall sliding and has no input do a Wall Hop
        else if (IsTouchingWall && movementInput.x == 0)
        {
            IsFacingRight = !IsFacingRight;
            rb.AddForce(new Vector2(FacingDirection * wallHopForce * wallHopDirection.x, wallHopForce * wallHopDirection.y), ForceMode2D.Impulse);

            OnPlayerWallHop?.Invoke();
        }

        // If the player is not grounded, is touching the wall, and there is input do a Wall Jump
        // IsTouchingWall used instead of IsWallSliding incase the Y velocity is positive
        else if (IsTouchingWall && movementInput.x != 0)
        {
            rb.AddForce(new Vector2(-FacingDirection * wallJumpForce * wallJumpDirection.x, wallJumpForce * wallJumpDirection.y), ForceMode2D.Impulse);

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

    /// <summary>
    /// Makes the Player slide.
    /// </summary>
    private void Slide()
    {
        IsSliding = true;
        slideTimeElapsed = 0;
        slideCurveSamplePoint = 0;
        timeOfLastSlide = Time.time;

        OnPlayerSlide?.Invoke();
    }

    /// <summary>
    /// If the Player is sliding, they will continue to slide as dictated by the sliding curve.
    /// </summary>
    private void SlidingBehaviour()
    {
        // Takes the facing direction, multiples that by the current point on the curve, and then multiplied by the slide
        rb.velocity = new Vector2(FacingDirection * (slideCurve.Evaluate(slideCurveSamplePoint) * slideForce), rb.velocity.y);

        slideCurveSamplePoint += Time.fixedDeltaTime / slideDuration;   // As fixedDeltaTime is usually ~< 0.01, dividing it by our duration offsets it to reach 1 by the end of the slide
        slideTimeElapsed += Time.fixedDeltaTime;
        IsSliding = slideTimeElapsed <= slideDuration;
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
        IsDashing = true;
        timeOfLastDash = Time.time;

        remainingDashes--;
        dashHeight = transform.position.y;

        rb.velocity = new Vector2(FacingDirection * dashSpeed, rb.velocity.y);
        OnPlayerDash?.Invoke();
    }

    private void Grapple()
    {
        IsGrappling = true;
        ResetActions();

        grapplePoint = GetClosestGrapplePoint();
        
        // Player is too close to the grapple
        if (Vector2.Distance(GrappleAnchorPosition, grapplePoint.transform.position) < grapplePoint.GrappleDistance.x)
        {
            Vector2 direction = (GrappleAnchorPosition - (Vector2)grapplePoint.transform.position).normalized;
            transform.position = (Vector2)grapplePoint.transform.position + direction * grapplePoint.GrappleDistance.x * 2;
        }

        grappleJoint.connectedAnchor = grapplePoint.transform.position;
        grappleJoint.enabled = true;

        OnPlayerGrapple?.Invoke(grapplePoint);
    }
    #endregion

    #region Grapple Methods
    public void AddValidGrapplePoint(GrapplePoint grapplePoint)
    {
        if (!grapplePoints.Contains(grapplePoint)) grapplePoints.Add(grapplePoint);
    }

    public void RemoveValidGrapplePoint(GrapplePoint grapplePoint)
    {
        grapplePoints.Remove(grapplePoint);
    }

    public GrapplePoint GetClosestGrapplePoint()
    {
        GrapplePoint grapplePoint = grapplePoints[0];
        float distanceToClosestPoint = Vector2.Distance(GrappleAnchorPosition, grapplePoint.transform.position);

        foreach(GrapplePoint point in grapplePoints)
        {
            float distanceToPoint = Vector2.Distance(GrappleAnchorPosition, point.transform.position);

            if (distanceToPoint < distanceToClosestPoint)
            {
                grapplePoint = point;
                distanceToClosestPoint = distanceToPoint;
            }
        }

        return grapplePoint;
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
    /// <param name="partition">Parameter not used, is required to subsribe to the event</param>
    private void LockPlayerInput(Partition partition = null)
    {
        IsLockedInput = true;
    }

    /// <summary>
    /// Un-Locks the players movement and actions inputs.
    /// </summary>
    private void UnLockPlayerInput()
    {
        IsLockedInput = false;
    }    

    /// <summary>
    /// Returns true if the Player is able to Move, Such as if the play is not sliding or dashing.
    /// </summary>
    private bool CanMove { get { return !IsSliding && !IsDashing; } }

    /// <summary>
    /// Returns true if the Player is able to Jump.
    /// </summary>
    private bool CanJump { get { return IsGrounded || IsGrappling || remainingAirJumps > 0 || IsTouchingWall; } }

    /// <summary>
    /// Returns true if the Player is able to Slide.
    /// </summary>
    private bool CanSlide { get { return IsGrounded && !IsSliding && !IsDashing && !IsGrappling && movementInput.x != 0 && TimeSinceLastSlide > slideCooldown; } }

    /// <summary>
    /// Returns true if the Player is able to Dash.
    /// </summary>
    private bool CanDash { get { return !IsSliding && !IsDashing && !IsGrappling && movementInput.x != 0 && remainingDashes > 0 && TimeSinceLastDash > dashCoolDown; } }

    /// <summary>
    /// Returns true if the Player is able to Move, Such as if the play is not sliding or dashing.
    /// </summary>
    private bool CanGrapple { get { return !IsGrappling && grapplePoints.Count > 0; } }


    private int FacingDirection { get { return IsFacingRight? 1 : -1; } }
    private float TimeSinceLastDash { get { return Time.time - timeOfLastDash; } }
    private float TimeSinceLastSlide { get { return Time.time - timeOfLastSlide; } }
    private float TimeSinceLastTrueGround { get { return Time.time - timeOfLastTrueIsGrounded; } }
    private float TimeSinceLastGroundedJump { get { return Time.time - timeOfLastGroundedJump; } }
    private float TimeSinceLastWallHit { get { return Time.time - timeOfLastWallHit; } }

    public Vector2 GrappleAnchorPosition { get { return (Vector2)transform.position + grappleJoint.anchor; } }
    #endregion

    #region Collision
    private void OnCollisionEnter2D(Collision2D collision)
    {

    }
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
#endif
    }
    #endregion
    #endregion
}