/// Trash object
/// Can be interacted with using the decoration system, but does not inherit from Decoration Object
/// Trash Objects are pulled at using the decoration selector, once pull far enough they take damage or are destroyed
/// 2 variants, Trash Object low health no collision, Trash Block Object high health collides with player
/// Once destroyed the items the trash contained is added to the Inventory Controller

using System;
using UnityEngine;

public class TrashObject : MonoBehaviour
{
    
    // Dictionary that contains the items that the trash will add to inventory after being destroyed
    // Item name - Item to be dropped upon destroying the trash
    // Vector2 - Min,Max. The minimum and maximum amount that can be dropped of the item. Leaving the max as 0 will make the min number the only outcome.
    [SerializeField] private SerializableDictionary<InventoryController.ItemNames, Vector2Int> trashBreakItems;
    

    private Vector2 startPosition; // Position that the sprite will use as an anchor
    private Vector2 targetPosition; // Position that the sprite is trying to reach

    private GameObject sprite; // Reference to child that contains the trash's sprite. Moved instead of the trash instead.
    [SerializeField] private GameObject blockingCollider; // Reference to child that contains collider that blocks player. Only TrashBlockObject has this.

    private Vector2 mouseStartPosition; // When begining a drag this holds the mouse original position
    private Vector2 mouseCurrentPosition;

    private bool isBeingPulled; // Is the player currently pulling this trash
    [SerializeField, Range(1,10)] private float pullMaxDistance; // Maximum distance the mouse can be before
    private float pullCurrentDistance;
    private Vector2 pullDirection;

    [SerializeField, Range(1,10)] private float visualPullMovementAllowed;

    private float vibrationIntensity;
    [SerializeField, Range(0,1)] float vibrationMax;

    [SerializeField, Range(1,6)] private int healthMax;
    private int health;
    [SerializeField, Range(0,60)] private int respawnCooldown; // How many minutes until respawn after pulling. If 0 then cannot respawn
    private bool isActive = true;
    private float respawnTime;

    [SerializeField, Range(1, 50)] private int dragMoveSpeed;

    [SerializeField] private SerializableDictionary<int, Sprite> damageDisplayedSprites; // When health == int change trash's sprite to the one in the dictionary.




    private void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>().gameObject;
    }

    void Start()
    {
        startPosition = transform.position; // Sets the trash's initial location. This is used as an anchor.
        Respawn(); // Ensures variables are set up
    }

    // Update is called once per frame
    void Update()
    {
        if (isBeingPulled)
        {
            mouseCurrentPosition = CameraController.Instance.MouseWorldPosition;
            pullCurrentDistance = Vector2.Distance(mouseStartPosition, mouseCurrentPosition);
            pullDirection = (mouseCurrentPosition - mouseStartPosition).normalized;
            if (pullCurrentDistance > pullMaxDistance)
            {
                health--;
                if (health == 0) EndPull(pullDirection); //Destroys the trash object, adds resources to inventory, and gives direction for the particle system
                else if (damageDisplayedSprites.ContainsKey(health)) sprite.GetComponent<SpriteRenderer>().sprite = damageDisplayedSprites[health];
                CancelPull();

            }
            else
            {
                // Closer mouse is to reaching pullMaxDistance vibrate the sprite
                vibrationIntensity = pullCurrentDistance / pullMaxDistance;
                float _vibrationAmount = (vibrationMax / 10) * vibrationIntensity;
                Vector2 _vibrationOffset = new Vector2(UnityEngine.Random.Range(-_vibrationAmount, _vibrationAmount), UnityEngine.Random.Range(-_vibrationAmount, _vibrationAmount));

                targetPosition = startPosition + ((mouseCurrentPosition - mouseStartPosition) / (visualPullMovementAllowed * 100));

                targetPosition = targetPosition += _vibrationOffset;
                sprite.transform.position = Vector2.Lerp(sprite.transform.position, targetPosition, dragMoveSpeed * Time.deltaTime);

            }
        }
        else if (Time.time > respawnTime && !isActive) Respawn();
        else sprite.transform.position = Vector2.Lerp(sprite.transform.position, targetPosition, dragMoveSpeed * Time.deltaTime);

    }

    public void StartPull()
    {
        mouseStartPosition = CameraController.Instance.MouseWorldPosition;
        isBeingPulled = true;
    }

    public void CancelPull()
    {
        targetPosition = startPosition;
        isBeingPulled = false;
    }

    public void EndPull(Vector2 _directionOfBrake) 
    {
        DecorationController.Instance.TrashBroken(trashBreakItems, transform.position, _directionOfBrake);
        //Destroy(this.gameObject);
        respawnTime = Time.time + (respawnCooldown*60);
        ToggleObject(false);
        isBeingPulled = false;
        targetPosition = startPosition;
    }
    
    private void Respawn()
    {
        sprite.transform.position = startPosition;
        targetPosition = startPosition;
        mouseStartPosition = Vector2.zero;
        health = healthMax;
        ToggleObject(true);
    }

    private void ToggleObject(bool _toggle)
    {
        isActive = _toggle;
        if (_toggle) sprite.GetComponent<SpriteRenderer>().color = Color.white;
        else sprite.GetComponent<SpriteRenderer>().color = Color.clear;
        GetComponent<BoxCollider2D>().enabled = _toggle;
        if (blockingCollider) blockingCollider.GetComponent<BoxCollider2D>().enabled = _toggle;
    }
    
}
