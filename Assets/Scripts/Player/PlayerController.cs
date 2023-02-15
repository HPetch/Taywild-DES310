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
    public event Action OnPlayerLand;
    public event Action OnPlayerSlide;
    #endregion

    #region Variables
    // State booleans
    public bool IsGrounded { get; private set; } = true;
    public bool IsSliding { get; private set; } = false;
    public bool IsDashing { get; private set; } = false;
    public bool IsLockedInput { get; private set; } = false;

    // Component References
    private Rigidbody2D rb;
    private Animator animator;
    private CapsuleCollider2D capsuleCollider;
    private SpriteRenderer spriteRenderer;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    private Vector2 movementInput = Vector2.zero;
    private bool isFacingRight = true;

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

    #region Slide
    [Space(10)]
    [Header("Slide Settings")]

    [Tooltip("The initial strength of the slide")]
    [Range(0, 25)]
    [SerializeField] private float slideForce = 15f;

    [Tooltip("The duration of the slide")]
    [Range(0, 2)]
    [SerializeField] private float slideDuration = 0.4f;

    [Space(5)]
    [Tooltip("Dictates the behaviour of the slide over it's duration")]
    [SerializeField] private AnimationCurve slideCurve;

    // Tracks if the slide action input has been queued
    private bool slideQueued = false;
    // Tracks how long the slide has lasted
    private float slideTimeElapsed = 0f;
    // Used to sample the slide curve
    private float slideCurveSamplePoint = 0f;
    #endregion

    #region Dash
    [Space(10)]
    [Header("Dash Settings")]
    [Tooltip("The speed of the dash")]
    [Range(0, 20)]
    [SerializeField] private float dashSpeed = 1f;

    [Tooltip("The duration the IsDashing state is true")]
    [Range(0, 2)]
    [SerializeField] private float dashDuration = 0.2f;

    [Tooltip("The cool down time between dashes")]
    [Range(0, 2)]
    [SerializeField] private float dashCoolDown = 1f;

    private float timeOfLastDash = 0f;
    private bool dashQueued = false;
    private float TimeSinceLastDash { get { return Time.time - timeOfLastDash; } }
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

        animator = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Start()
    {
        TransitionController.Instance.OnTransitionStart += LockPlayerInput;
        TransitionController.Instance.OnTransitionEnd += UnLockPlayerInput;
    }
    #endregion

    #region Updates
    private void Update()
    {
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        CheckMovementDirection();
        IsGrounded = CheckIfPlayerIsGrounded();

        HandleActionInput();
        UpdateAnimationController();
    }

    private void FixedUpdate()
    {
        SlidingBehaviour();

        if (CanMove) ApplyMovement();

    }
    #endregion

    private void CheckMovementDirection()
    {
        if ((isFacingRight && movementInput.x < 0) || (!isFacingRight && movementInput.x > 0)) isFacingRight = !isFacingRight;
    }

    private void ApplyMovement()
    {
        rb.velocity = new Vector2(movementInput.x * moveSpeed, rb.velocity.y);
    }

    #region ActionInputs
    /// <summary>
    /// Executes the action inputs if they are available and the player input is not locked.
    /// </summary>
    private void HandleActionInput()
    {
        if (IsDashing && TimeSinceLastDash > dashDuration) IsDashing = false;

        // If the dialogue or transition systems have locked the player's input
        if (IsLockedInput) return;

        ResolveQue();

        if (Input.GetButtonDown("Jump"))
        {
            if (CanJump) Jump();
            else jumpQueued = true;
        }

        if (Input.GetButtonDown("Slide"))
        {
            if (CanSlide) Slide();
            else slideQueued = true;
        }

        if (Input.GetButtonDown("Dash"))
        {
            if (CanDash) Dash();
            else dashQueued = true;
        }
    }

    /// <summary>
    /// Execute the queued action input if the action becomes available.
    /// </summary>
    void ResolveQue()
    {
        // If you're not holding the button after quing an action, assume you don't wanna do it

        if (jumpQueued)
        {
            if (!Input.GetButton("Jump")) jumpQueued = false;
            else if (CanJump) Jump();
        }

        if (slideQueued)
        {
            if (!Input.GetButton("Slide")) slideQueued = false;
            else if (CanSlide) Slide();
        }

        if (dashQueued)
        {
            if (!Input.GetButton("Dash")) dashQueued = false;
            else if (CanDash) Dash();
        }
    }
    #endregion

    #region Actions
    /// <summary>
    /// Makes the player jump, if they're grounded it's a grounded jump, else it's an air jump.
    /// </summary>
    private void Jump()
    {
        // If player is grounded do a ground jump
        if (IsGrounded)
        {
            IsGrounded = false;
            rb.velocity = new Vector2(rb.velocity.x, groundedJumpForce);
            timeOfLastGroundedJump = Time.time;
            OnPlayerGroundJump?.Invoke();
        }
        // Else it's an air jump
        else
        {
            remainingAirJumps--;
            rb.velocity = new Vector2(rb.velocity.x, airJumpForce);
            OnPlayerAirJump?.Invoke();
        }

        //animator.Play("Player_Jump");
        //source.PlayOneShot(jumpClip);
        OnPlayerJump?.Invoke();
    }

    /// <summary>
    /// Makes the Player slide.
    /// </summary>
    private void Slide()
    {
        //source.PlayOneShot(slideClip);

        IsSliding = true;
        slideTimeElapsed = 0;
        slideCurveSamplePoint = 0;

        //animator.Play("Player_Slide");

        OnPlayerSlide?.Invoke();
    }

    /// <summary>
    /// If the Player is sliding, they will continue to slide as dictated by the sliding curve.
    /// </summary>
    private void SlidingBehaviour()
    {
        if (IsSliding)
        {
            // Takes the facing direction, multiples that by the current point on the curve, and then multiplied by the slide
            rb.velocity = new Vector2(FacingDirection * (slideCurve.Evaluate(slideCurveSamplePoint) * slideForce), rb.velocity.y);

            slideCurveSamplePoint += Time.fixedDeltaTime / slideDuration;   // As fixedDeltaTime is usually ~< 0.01, dividing it by our duration offsets it to reach 1 by the end of the slide
            slideTimeElapsed += Time.fixedDeltaTime;
            IsSliding = slideTimeElapsed <= slideDuration;
        }
    }

    private void Dash()
    {
        IsDashing = true;
        timeOfLastDash = Time.time;

        Debug.Log("Dash");
        rb.velocity = new Vector2(FacingDirection * dashSpeed, rb.velocity.y);
    }
    #endregion

    #region Visuals
    /// <summary>
    /// Passes the state booleans.
    /// </summary>
    void UpdateAnimationController()
    {
        spriteRenderer.flipX = FacingDirection == 1 ? false : true;
        animator.SetBool("Running", Mathf.Abs(movementInput.x) > 0.2f);
        animator.SetBool("Grounded", IsGrounded);
    }
    #endregion

    #region Utilities
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
    /// Uses a BoxCast on the platform layer to determine if the player is grounded, takes into consideration coyote time.
    /// </summary>
    /// <returns> Returns true if the player is either grounded or coyote grounded.</returns>
    private bool CheckIfPlayerIsGrounded()
    {
        bool isGroundedThisFrame = Physics2D.BoxCast(capsuleCollider.bounds.center, capsuleCollider.bounds.size, 0f, Vector2.down, 0.001f, platformLayerMask);
        DrawDebugInfo();

        // If the player is grounded this frame
        if (isGroundedThisFrame)
        {
            // And coyote time has passed since the last ground jump
            if (Time.time - timeOfLastGroundedJump > coyoteTime)
            {
                // This is a true grounded
                timeOfLastTrueIsGrounded = Time.time;

                // If the player was not grounded on the previous frame but is grounded this frame, then they have landed
                if (!IsGrounded)
                {
                    // Reset air jumps
                    remainingAirJumps = maxAirJumps;
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
        else if (Time.time - timeOfLastTrueIsGrounded < coyoteTime && Time.time - timeOfLastGroundedJump > coyoteTime)
        {
            isGroundedThisFrame = true;
        }

        return isGroundedThisFrame;
    }

    /// <summary>
    /// Returns true if the Player is able to Jump.
    /// </summary>
    private bool CanJump { get { return IsGrounded || remainingAirJumps > 0; } }

    /// <summary>
    /// Returns true if the Player is able to Slide.
    /// </summary>
    private bool CanSlide { get { return IsGrounded && !IsSliding && !IsDashing && movementInput.x != 0; } }

    /// <summary>
    /// Returns true if the Player is able to Dash.
    /// </summary>
    private bool CanDash { get { return !IsSliding && !IsDashing && (Time.time - timeOfLastDash > dashCoolDown) && movementInput.x != 0; } }

    /// <summary>
    /// Returns true if the Player is able to Move, Such as if the play is not sliding or dashing.
    /// </summary>
    private bool CanMove { get { return !IsSliding && !IsDashing; } }

    private int FacingDirection { get { return isFacingRight? 1 : -1; } }
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