using UnityEngine;
using MinMaxSlider;

public class GrapplePoint : MonoBehaviour
{
    private enum GrapplePointStates {  INACTIVE, READY, ACTIVE}

    [MinMaxSlider(1, 10)]
    [SerializeField] private Vector2 grappleDistance = new Vector2(2, 6);

    private SpriteRenderer spriteRenderer;
    private PlayerController player;

    private GrapplePointStates grapplePointState = GrapplePointStates.INACTIVE;
    

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        player = PlayerController.Instance;

        player.OnPlayerGrapple += PlayerGrappleStart;
        player.OnPlayerGrappleEnd += PlayerGrappleEnd;
    }

    private void Update()
    {
        float distanceToplayer = Vector2.Distance(player.GrappleAnchorPosition, transform.position);

        switch (grapplePointState)
        {
            case GrapplePointStates.INACTIVE:
                if (distanceToplayer > grappleDistance.x && distanceToplayer < grappleDistance.y)
                {
                    grapplePointState = GrapplePointStates.READY;
                    spriteRenderer.color = Color.yellow;
                    player.AddValidGrapplePoint(this);
                }
                break;

            case GrapplePointStates.READY:
                if (distanceToplayer < grappleDistance.x || distanceToplayer > grappleDistance.y)
                {
                    grapplePointState = GrapplePointStates.INACTIVE;
                    spriteRenderer.color = Color.white;
                    player.RemoveValidGrapplePoint(this);
                }
                break;

            case GrapplePointStates.ACTIVE:
                break;
        }
    }

    private void PlayerGrappleStart(GrapplePoint grapplePoint)
    {
        if (grapplePoint == this)
        {
            spriteRenderer.color = Color.green;
            grapplePointState = GrapplePointStates.ACTIVE;
        }
    }

    private void PlayerGrappleEnd()
    {
        if (grapplePointState == GrapplePointStates.ACTIVE)
        {
            grapplePointState = GrapplePointStates.READY;
            spriteRenderer.color = Color.yellow;
        }
    }
}