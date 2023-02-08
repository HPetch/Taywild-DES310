using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    #region Events
    public event Action OnPlayerJump;
    public event Action OnPlayerLand;
    public event Action OnPlayerSlide;
    //public event Action OnInteract;
    #endregion

    #region Variables
    public bool Grounded { get; private set; } = true;
    public bool Sliding { get; private set; } = false;

    private Rigidbody2D rb;
    private Animator animator;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10;

    [Space(10)]
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 20;
    [SerializeField] private int maxJumps = 2;
    private int remainingJumps = 1;
    private bool jumpQueued = false;

    [Space(10)]
    [Header("Slide Settings")]
    [SerializeField] private float slideForce;
    [SerializeField] private float slideDuration;
    [Space(5)]
    [SerializeField] private AnimationCurve slideCurve;
    private bool slideQueued = false;
    private float slideTimeElapsed;
    private float slideCurveSamplePoint = 0f;
    #endregion

    #region Functions
    #region Initialisation
    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }
    #endregion

    private void Update()
    {
        HandleInput();
        UpdateAnimationController();
    }

    private void FixedUpdate()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        SlidingBehaviour();

        if (!Sliding)
        {
            rb.velocity = new Vector2(input.x * moveSpeed, rb.velocity.y);
        }
    }

    private void HandleInput()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));


        if (Input.GetButtonDown("Jump"))
        {
            if (remainingJumps > 0)
            {
                Jump();
            }
        }

        if (Input.GetButtonDown("Slide"))
        {
            if (Grounded && !Sliding)
            {
                Slide();
            }

        }

        ResolveQue();

        transform.localScale = new Vector3(
            Mathf.Abs(input.x) > 0.2f ? (input.x < 0 ? -1 : 1) : transform.localScale.x,
            transform.localScale.y,
            transform.localScale.z
            );
    }

    void ResolveQue()
    {
        // If you're not holding the button after quing an action, assume you don't wanna do it

        if (Grounded)
        {
            if (jumpQueued)
            {
                if (!Input.GetButton("Jump"))
                {
                    jumpQueued = false;
                }
                else
                {
                    Jump();
                }
            }

            if (slideQueued)
            {
                if (!Input.GetButton("Slide"))
                {
                    slideQueued = false;
                }
                else
                {
                    Slide();
                }
            }
        }
    }

    private void Jump()
    {
        remainingJumps--;

        Grounded = false;
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        //animator.Play("Player_Jump");
        //source.PlayOneShot(jumpClip);

        OnPlayerJump?.Invoke();
    }

    private void Slide()
    {
        //source.PlayOneShot(slideClip);

        Sliding = true;
        slideTimeElapsed = 0;
        slideCurveSamplePoint = 0;

        //animator.Play("Player_Slide");

        OnPlayerSlide?.Invoke();
    }

    void SlidingBehaviour()
    {
        if (Sliding)
        {
            // Takes the facing direction, multiples that by the current point on the curve, and then multiplied by the slide
            rb.velocity = new Vector2(transform.localScale.x * (slideCurve.Evaluate(slideCurveSamplePoint) * slideForce), rb.velocity.y);

            slideCurveSamplePoint += Time.fixedDeltaTime / slideDuration;   // As fixedDeltaTime is usually ~< 0.01, dividing it by our duration offsets it to reach 1 by the end of the slide
            slideTimeElapsed += Time.fixedDeltaTime;
            Sliding = slideTimeElapsed <= slideDuration;
        }
    }

    void UpdateAnimationController()
    {
        animator.SetBool("Running", Mathf.Abs(Input.GetAxis("Horizontal")) > 0.2f);
        animator.SetBool("Grounded", Grounded);
    }

    #region Collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ground"))
        {
            if (transform.position.y + 0.1f > collision.contacts[0].point.y && Mathf.Abs(collision.contacts[0].point.x - transform.position.x) < 0.2f)
            {
                Grounded = true;
                remainingJumps = maxJumps;

                OnPlayerLand?.Invoke();
            }
        }
    }
    #endregion
    #endregion
}