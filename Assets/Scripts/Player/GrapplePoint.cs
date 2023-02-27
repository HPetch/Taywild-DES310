using UnityEngine;
using MinMaxSlider;

public class GrapplePoint : MonoBehaviour
{
    private enum GrapplePointStates { INACTIVE, READY, ACTIVE }

    public bool IsGrappleValid { get; private set; }

    [field: MinMaxSlider(1, 10)]
    [field: SerializeField] public Vector2 GrappleDistance { get; set; } = new Vector2(2, 6);

    [MinMaxSlider(0, 180)]
    [SerializeField] private Vector2 grappleAngle = new Vector2(15, 165);

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
        float angleToPlayer = Vector2.Angle(transform.position, player.GrappleAnchorPosition);
        IsGrappleValid =    distanceToplayer < GrappleDistance.y && 
                            angleToPlayer > grappleAngle.x && 
                            angleToPlayer < grappleAngle.y && 
                            transform.position.y > player.transform.position.y;

        switch (grapplePointState)
        {
            case GrapplePointStates.INACTIVE:
                if (IsGrappleValid)
                {
                    grapplePointState = GrapplePointStates.READY;
                    spriteRenderer.color = Color.yellow;
                    player.AddValidGrapplePoint(this);
                }
                break;

            case GrapplePointStates.READY:
                if (!IsGrappleValid)
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