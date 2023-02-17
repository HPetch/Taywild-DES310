using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectController : MonoBehaviour
{
    private PlayerController player = null;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Ground Jump Effects")]
    [SerializeField] private GameObject groundJumpParticleEffect;
    [SerializeField] private AudioClip groundJumpAudioClip;

    [Header("Air Jump Effects")]
    [SerializeField] private GameObject airJumpParticleEffect;
    [SerializeField] private AudioClip airJumpAudioClip;

    [Header("Wall Jump Effects")]
    [SerializeField] private GameObject wallJumpParticleEffect;
    [SerializeField] private AudioClip wallJumpAudioClip;

    [Header("Wall Hop Effects")]
    [SerializeField] private GameObject wallHopParticleEffect;
    [SerializeField] private AudioClip wallHopAudioClip;

    [Header("Wall Hit Effects")]
    [SerializeField] private GameObject wallHitParticleEffect;
    [SerializeField] private AudioClip wallHitAudioClip;

    [Header("Wall Slide Effects")]
    [SerializeField] private GameObject wallSlideParticleEffect;
    [SerializeField] private AudioClip wallSlideAudioClip;

    [Header("Land Effects")]
    [SerializeField] private GameObject landParticleEffect;
    [SerializeField] private AudioClip landAudioClip;

    [Header("Slide Effects")]
    [SerializeField] private GameObject slideParticleEffect;
    [SerializeField] private AudioClip slideAudioClip;

    [Header("Dash Effects")]
    [SerializeField] private GameObject dashParticleEffect;
    [SerializeField] private AudioClip dashAudioClip;

    private void Awake()
    {
        player = GetComponentInParent<PlayerController>();

        rb = GetComponentInParent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        player.OnPlayerJump += PlayerJump;
        player.OnPlayerGroundJump += PlayerGroundJump;
        player.OnPlayerAirJump += PlayerAirJump;
        player.OnPlayerWallHop += PlayerWallHop;
        player.OnPlayerWallJump += PlayerWallJump;
        player.OnPlayerWallHit += PlayerWallHit;
        player.OnPlayerWallSlideStart+= PlayerWallSlideStart;
        player.OnPlayerWallSlideEnd += PlayerWallSlideEnd;
        player.OnPlayerLand += PlayerLanded;
        player.OnPlayerSlide += PlayerSlide;
        player.OnPlayerDash += PlayerDash;
    }

    private void Update()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        spriteRenderer.flipX = !player.IsFacingRight;
        if (player.IsWallSliding || player.IsWallStuck) spriteRenderer.flipX = player.IsFacingRight;

        animator.SetBool("Running", Mathf.Abs(rb.velocity.x) > 0.2f);
        animator.SetBool("Grounded", player.IsGrounded);
        animator.SetBool("Sliding", player.IsSliding);
        animator.SetFloat("Y_Velocity", rb.velocity.y);
    }

    private void PlayerJump()
    {

    }

    private void PlayerGroundJump()
    {
        Instantiate(groundJumpParticleEffect, player.transform.position, Quaternion.identity);
        CameraController.Instance.PunchOut(0.1f, 20f);
    }

    private void PlayerAirJump()
    {

    }

    private void PlayerWallHop()
    {
        CameraController.Instance.PunchOut(0.05f, 10f);
    }

    private void PlayerWallJump()
    {
        Transform leafEffect = Instantiate(wallJumpParticleEffect, player.transform.position, Quaternion.identity).transform;
        leafEffect.localScale = player.IsFacingRight ? leafEffect.localScale : new Vector3(-1, 1, 1);

        CameraController.Instance.PunchOut(0.1f, 20f);
    }

    private void PlayerWallHit()
    {
        CameraController.Instance.Shake(0.1f, 0.005f);
        CameraController.Instance.PunchIn(0.3f, 20f, true);
    }

    private void PlayerWallSlideStart()
    {
        CameraController.Instance.PunchIn(0, 5f);
    }

    private void PlayerWallSlideEnd()
    {

    }

    private void PlayerLanded()
    {
        CameraController.Instance.Shake(0.2f, 0.02f);
    }

    private void PlayerSlide()
    {
        CameraController.Instance.PunchIn(0.2f, 10f);
    }


    private void PlayerDash()
    {
        CameraController.Instance.PunchOut(0.1f, 10f);
    }
}