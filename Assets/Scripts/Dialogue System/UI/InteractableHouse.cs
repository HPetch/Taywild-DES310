using UnityEngine;

public class InteractableHouse : MonoBehaviour
{
    [SerializeField] private SpriteRenderer houseExterior;
    [SerializeField] private SpriteRenderer door;
    [SerializeField] private Sprite closedDoor;
    [SerializeField] private Sprite ajarDoor;
    [SerializeField] private CanvasGroup interactCanvasGroup;

    [Space(10)]
    [SerializeField] private GameObject character;
    [SerializeField] private GameObject houseColliders;

    [Space(10)]
    [SerializeField] private float zoomLevel;
    [SerializeField] private bool cameraFollowPlayer;

    private bool isNextToDoor = false;
    private bool isInHouse = false;

    private void Awake()
    {
        interactCanvasGroup.alpha = 0;

        isNextToDoor = false;
        isInHouse = false;

        character.SetActive(false);
        houseColliders.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Interact") && PlayerController.Instance.IsGrounded && isNextToDoor)
        {
            if (!isInHouse) EnterHouse();
            else ExitHouse();
        }
    }

    private void EnterHouse()
    {
        character.SetActive(true);
        houseColliders.SetActive(true);

        LeanTween.cancel(houseExterior.gameObject);
        LeanTween.alpha(houseExterior.gameObject, 0, 0.15f);

        LeanTween.cancel(door.gameObject);
        LeanTween.alpha(door.gameObject, 0.4f, 0.15f);

        HouseZoom();

        isInHouse = true;
        PlayerController.Instance.SetCurrentHouse(this);
    }

    public void HouseZoom()
    {
        CameraController.Instance.Zoom(zoomLevel);
        if (!cameraFollowPlayer) CameraController.Instance.SetTarget(door.transform);
    }

    private void ExitHouse()
    {
        character.SetActive(false);
        houseColliders.SetActive(false);

        LeanTween.cancel(houseExterior.gameObject);
        LeanTween.alpha(houseExterior.gameObject, 1, 0.15f);

        LeanTween.cancel(door.gameObject);
        LeanTween.alpha(door.gameObject, 1, 0.15f);

        CameraController.Instance.ResetTarget();
        CameraController.Instance.ZoomToPartitionSize();

        isInHouse = false;
        PlayerController.Instance.SetCurrentHouse(null);
    }

    #region Collision
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !DialogueController.Instance.IsConversing && !DecorationController.Instance.isEditMode)
        {
            LeanTween.cancel(interactCanvasGroup.gameObject);
            LeanTween.alphaCanvas(interactCanvasGroup, 1, 0.15f);

            door.sprite = ajarDoor;
            isNextToDoor = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            LeanTween.cancel(interactCanvasGroup.gameObject);
            LeanTween.alphaCanvas(interactCanvasGroup, 0, 0.15f);

            door.sprite = closedDoor;
            isNextToDoor = false;
        }
    }
    #endregion
}