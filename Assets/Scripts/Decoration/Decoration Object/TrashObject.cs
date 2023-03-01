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

    private GameObject spriteArm; // Reference to child that has a child which is the sprite. This is moved, but not vibrated.
    private GameObject sprite; // Reference to child that contains the trash's sprite. This is vibrated.
    [SerializeField] private GameObject blockingCollider; // Reference to child that contains collider that blocks player. Only TrashBlockObject has this.

    private Vector2 mouseStartPosition; // When begining a drag this holds the mouse original position
    private Vector2 mouseCurrentPosition;

    private bool isBeingPulled; // Is the player currently pulling this trash
    [SerializeField, Range(0.1f,0.5f)] private float pullBreakDistance; // Maximum distance the mouse can be before
    private Vector2 pullDirection;

    [SerializeField, Range(0.5f,5)] private float pullMoveResistance;
    private float pullBreakTime;
    private bool isTryingToBreak;

    private float vibrationIntensity;
    [SerializeField, Range(0,0.2f)] float vibrationMax;
    [SerializeField, Range(0.1f, 10)] float vibrationSpeed;

    [SerializeField, Range(1,6)] private int healthMax;
    private int health;

    // How many minutes until respawn after pulling. If 0 then cannot respawn
    [SerializeField, Range(0,60)] private int respawnCooldown;
    private bool isActive = true;
    private float respawnTime;
    [SerializeField] LayerMask respawnBlockLayerMask;

    [SerializeField, Range(1, 10)] private int dragMoveSpeed;

    // When health == int change trash's sprite to the one in the dictionary.
    [SerializeField] private SerializableDictionary<int, Sprite> damageDisplayedSprites;




    private void Awake()
    {
        sprite = GetComponentInChildren<SpriteRenderer>().gameObject;
        spriteArm = sprite.transform.parent.gameObject;
    }

    void Start()
    {
        // Sets the trash's initial location. This is used as an anchor for sprite arm.
        startPosition = transform.position;

        // Ensures variables are set up
        Respawn(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (isBeingPulled)
        {
            mouseCurrentPosition = CameraController.Instance.MouseWorldPosition;
            
            pullDirection = (mouseCurrentPosition - mouseStartPosition).normalized;
            
            

            targetPosition = startPosition + ((mouseCurrentPosition - mouseStartPosition) / (pullMoveResistance * 10)); // The position that the sprite arm will be trying to move to

            // Slows movement of sprite arm as it gets closer to breaking to look like it's resisting
            float dragMoveSpeedSlowdown = Mathf.Clamp(Mathf.Abs(PullCurrentDistance()-pullBreakDistance)/pullBreakDistance,0.01f,1.0f);

            // If the trash is trying to break stops movement to prevent weird movement
            if (!isTryingToBreak) 
            {
                spriteArm.transform.position = Vector2.Lerp(spriteArm.transform.position, targetPosition, dragMoveSpeed * dragMoveSpeedSlowdown * Time.deltaTime);

                // Closer the sprite is to breaking vibrate with extra intensity
                vibrationIntensity = (PullCurrentDistance() / pullBreakDistance) * vibrationMax; 
            }

            Vector2 _vibrationOffset = new Vector2(UnityEngine.Random.Range(-vibrationIntensity, vibrationIntensity), UnityEngine.Random.Range(-vibrationIntensity, vibrationIntensity));
            
            // Vibrates sprite independent of sprite arm to prevent funky math
            sprite.transform.localPosition = Vector2.Lerp(sprite.transform.localPosition, _vibrationOffset, vibrationSpeed * Time.deltaTime);

            // If sprite arm is past break distance then start trying to break the trash
            if (PullCurrentDistance() > pullBreakDistance)
            {
                // Checks if a break attmempt has started yet
                if (pullBreakTime < Time.time && !isTryingToBreak)
                {
                    pullBreakTime = Time.time + 0.5f;
                    isTryingToBreak = true;
                }
                else if (pullBreakTime < Time.time && isTryingToBreak)
                {
                    health--;
                    if (health == 0) EndPull(pullDirection); //Destroys the trash object, adds resources to inventory, and gives direction for the particle system
                    else if (damageDisplayedSprites.ContainsKey(health)) sprite.GetComponent<SpriteRenderer>().sprite = damageDisplayedSprites[health];
                    CancelPull();
                }
            }
            // If player have moved their mouse within the break distance then cancel the break attempt
            if (Vector2.Distance(startPosition, targetPosition) < pullBreakDistance) isTryingToBreak = false;
        }
        // If the is able to respawn then wait until the correct time then respawn
        else if (Time.time > respawnTime && !isActive && respawnCooldown != 0) Respawn();
        

    }

    // Called by decoration controller when the decoration selector interacts with the trash
    public void StartPull()
    {
        mouseStartPosition = CameraController.Instance.MouseWorldPosition;
        spriteArm.transform.position = startPosition;
        sprite.transform.localPosition = Vector2.zero;
        targetPosition = startPosition;
        isBeingPulled = true;
        isTryingToBreak = false;
    }

    // Called by decoration controller when the player releases the mouse button. Also called when removing health from trash that doesn't break.
    public void CancelPull()
    {
        isBeingPulled = false;
        spriteArm.transform.position = startPosition;
        sprite.transform.localPosition = Vector2.zero;
        targetPosition = startPosition;
        isTryingToBreak = false;
    }

    // Called when trash has been broken
    public void EndPull(Vector2 _directionOfBrake) 
    {
        // Tells the inventory contoller what items were dropped by the trash. Also gives trash's position and direction for particle system.
        DecorationController.Instance.TrashBroken(trashBreakItems, spriteArm.transform.position, _directionOfBrake);
        respawnTime = Time.time + (respawnCooldown*60);
        ToggleObject(false);
        isBeingPulled = false;
        isTryingToBreak = false;
        targetPosition = startPosition;
    }
    
    // Called when respawn timer reaches the correct time and the trash is able to respawn.
    private void Respawn()
    {
        bool _canRespawn = true;

        //Checks if the player or any furniture is not blocking it's respawn
        Vector2 _hitPosition = GetComponent<BoxCollider2D>().transform.position;
        Vector2 _hitSize = GetComponent<BoxCollider2D>().size;
        float _hitRotation = GetComponent<BoxCollider2D>().transform.rotation.eulerAngles.z;
        if (Physics2D.OverlapBox(_hitPosition, _hitSize, _hitRotation, respawnBlockLayerMask)) _canRespawn = false;

        // Checks that if the trash has a blocking collider then the player or any furniture is not blocking it's respawn
        if (blockingCollider)
        {
            Vector2 _blockPosition = blockingCollider.GetComponent<BoxCollider2D>().transform.position;
            Vector2 _blockSize = blockingCollider.GetComponent<BoxCollider2D>().size;
            float _blockRotation = blockingCollider.GetComponent<BoxCollider2D>().transform.rotation.eulerAngles.z;
            if (Physics2D.OverlapBox(_blockPosition, _blockSize, _blockRotation, respawnBlockLayerMask)) _canRespawn = false;
        }
        if (_canRespawn)
        {
            spriteArm.transform.position = startPosition;
            sprite.transform.localPosition = Vector2.zero;
            targetPosition = startPosition;
            mouseStartPosition = Vector2.zero;
            health = healthMax;
            ToggleObject(true);
        }
        // If the trash can't respawn then reset it's respawn timer
        else respawnTime = Time.time + (respawnCooldown * 60);

    }

    // Called when the trash is pulled or respawned. Makes it appear as if it has been destroyed/respawned.
    private void ToggleObject(bool _toggle)
    {
        isActive = _toggle;
        if (_toggle) sprite.GetComponent<SpriteRenderer>().color = Color.white;
        else sprite.GetComponent<SpriteRenderer>().color = Color.clear;
        GetComponent<BoxCollider2D>().enabled = _toggle;
        if (blockingCollider) blockingCollider.GetComponent<BoxCollider2D>().enabled = _toggle;
    }
    




    private float PullCurrentDistance()
    {
        return Vector2.Distance(startPosition, spriteArm.transform.position);
    }
}
