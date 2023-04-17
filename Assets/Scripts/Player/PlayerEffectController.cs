using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectController : MonoBehaviour
{
    public enum SURFACE_TYPE
    {
        Generic, Sky, Branch, Roof
    }
    private SURFACE_TYPE currentSurface = SURFACE_TYPE.Generic;

    private PlayerController player = null;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    

    [Header("Ground Jump Effects")]
    [SerializeField] private GameObject groundJumpParticleEffect;
    [SerializeField] private AudioClip[] groundJumpAudioClips;

    [Header("Air Jump Effects")]
    [SerializeField] private GameObject airJumpParticleEffect;
    [SerializeField] private AudioClip[] airJumpAudioClips;

    [Header("Wall Jump Effects")]
    [SerializeField] private GameObject wallJumpParticleEffect;
    [SerializeField] private AudioClip[] wallJumpAudioClips;

    [Header("Wall Hop Effects")]
    [SerializeField] private GameObject wallHopParticleEffect;
    [SerializeField] private AudioClip[] wallHopAudioClips;

    [Header("Wall Hit Effects")]
    [SerializeField] private GameObject wallHitParticleEffect;
    [SerializeField] private AudioClip[] wallHitAudioClips;

    [Header("Wall Slide Effects")]
    [SerializeField] private GameObject wallSlideParticleEffect;
    [SerializeField] private AudioClip[] wallSlideAudioClips;

    [Header("Land Effects")]
    [SerializeField] private GameObject landParticleEffect;
    [SerializeField] private AudioClip[] landAudioClips;
    

    [Header("Dash Effects")]
    [SerializeField] private GameObject dashParticleEffect;
    [SerializeField] private AudioClip[] dashAudioClips;

    [Header("Run Effects")]
    [SerializeField] private AudioClip[] runGenericClips;
    [SerializeField] private AudioClip[] runSkyClips;
    [SerializeField] private AudioClip[] runBranchClips;
    [SerializeField] private AudioClip[] runRoofClips;

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
        player.OnPlayerWallSlide += PlayerWallSlideStart;
        player.OnPlayerWallSlideEnd += PlayerWallSlideEnd;
        player.OnPlayerLand += PlayerLanded;
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
        animator.SetBool("Gliding", player.IsGliding);
        animator.SetFloat("Y_Velocity", rb.velocity.y);
    }

    private void PlayerJump()
    {

    }

    private void PlayerGroundJump()
    {
        Instantiate(groundJumpParticleEffect, player.transform.position, Quaternion.identity);
        CameraController.Instance.PunchOut(0.1f, 10f);

        //Play a random audio clip from range.
        AudioClip clip = groundJumpAudioClips[Random.Range(0, groundJumpAudioClips.Length - 1)];
        AudioController.Instance.PlaySound(clip,0.4f,true);
    }

    private void PlayerAirJump()
    {
        Instantiate(airJumpParticleEffect, player.transform.position + new Vector3(0, 1, 0), airJumpParticleEffect.transform.rotation);

        //Play a random audio clip from range.
        AudioClip clip = airJumpAudioClips[Random.Range(0, airJumpAudioClips.Length - 1)];
        AudioController.Instance.PlaySound(clip, false);
    }

    private void PlayerWallHop()
    {
        Transform leafEffect = Instantiate(wallHopParticleEffect, player.transform.position, Quaternion.identity).transform;
        leafEffect.localScale = !player.IsFacingRight ? leafEffect.localScale : new Vector3(-1, 1, 1);

        CameraController.Instance.PunchOut(0.05f, 5f);
    }

    private void PlayerWallJump()
    {
        Transform leafEffect = Instantiate(wallJumpParticleEffect, player.transform.position, Quaternion.identity).transform;
        leafEffect.localScale = player.IsFacingRight ? leafEffect.localScale : new Vector3(-1, 1, 1);

        CameraController.Instance.PunchOut(0.1f, 10f);

        //Play a random audio clip from range.
        AudioClip clip = wallJumpAudioClips[Random.Range(0, wallJumpAudioClips.Length - 1)];
        AudioController.Instance.PlaySound(clip, false);
    }

    private void PlayerWallHit()
    {
        CameraController.Instance.Shake(0.1f, 0.005f);
        CameraController.Instance.PunchIn(0.3f, 10f, true);
    }

    private void PlayerWallSlideStart()
    {
        CameraController.Instance.PunchIn(0.1f, 5f);
    }

    private void PlayerWallSlideEnd()
    {

    }

    private void PlayerLanded(GameObject _platform)
    {
        CameraController.Instance.Shake(0.2f, 0.02f);


        //Store current surface below player
        RaycastHit2D ray;
        //----Get raycast hit to whatever collider is below it, access SurfaceBroadcaster.cs and store the surface type.----

        
        /*if (ray.transform.GetComponent<SurfaceBroadcaster>() != null)
        {
          currentSurface = ray.transform.GetComponent<SurfaceBroadcaster>().getSurfaceType();
        }*/

    }

    private void PlayerDash()
    {
        Transform dashEffect = Instantiate(dashParticleEffect, player.transform.position + dashParticleEffect.transform.position, dashParticleEffect.transform.rotation).transform;
        dashEffect.localScale = !player.IsFacingRight ? dashEffect.localScale : new Vector3(-1, 1, 1);

        CameraController.Instance.PunchOut(0.1f, 10f);
    }
    private void PlayerRun()
    {
        //Define local variable
        AudioClip clip = null;

        //Get the right clip to use based on surface type
        switch (currentSurface)
        {
            case SURFACE_TYPE.Generic:
                clip = runGenericClips[Random.Range(0, runGenericClips.Length - 1)];
                break;
            case SURFACE_TYPE.Sky:
                clip = runSkyClips[Random.Range(0, runSkyClips.Length - 1)];
                break;
            case SURFACE_TYPE.Branch:
                clip = runBranchClips[Random.Range(0, runBranchClips.Length - 1)];
                break;
            case SURFACE_TYPE.Roof:
                clip = runRoofClips[Random.Range(0, runRoofClips.Length - 1)];
                break;
            default:
                Debug.LogWarning("This surface has no valid type - playing generic sound instead");
                clip = runGenericClips[Random.Range(0, runGenericClips.Length - 1)];
                break;
        }

        //Play the sound
        AudioController.Instance.PlaySound(clip,true);
    }

}