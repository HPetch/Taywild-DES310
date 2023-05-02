/// Pickup object
/// Can be interacted with using the decoration system, but does not inherit from Decoration Object
/// pickup objects are pulled at using the decoration selector, once pull far enough they take damage or are destroyed
/// 2 variants, Pickup Object low health no collision, Pickup Block Object high health collides with player
/// Once destroyed the items the pickup object contained is added to the Inventory Controller

using Unity.VisualScripting;
using UnityEngine;

public class PickupObject : MonoBehaviour
{
    #region Variables
    [SerializeField, Range(1, 6)] private int healthMax; // How many pulls until the object breaks.
    private int health; // Current health, the higher this is the more difficult objects are to damage.

    private Vector2 startPosition; // Position that the sprite will use as an anchor
    private Vector2 targetPosition; // Position that the sprite is trying to reach

    private GameObject spriteArmRef; // Reference to child that has a child which is the sprite. This is moved, but not vibrated.
    private GameObject spriteRef; // Reference to child that contains the pickup's sprite. This is vibrated.
    [SerializeField] private Sprite sprite;
    [SerializeField] private Sprite spriteBase;
    [SerializeField] private GameObject blockingCollider; // Reference to child that contains collider that blocks player. Only PickupBlockObject has this.

    private Vector2 mouseStartPosition; // When begining a drag this holds the mouse original position
    private Vector2 mouseCurrentPosition;

    private bool isBeingPulled; // Is the player currently pulling this pickup
    [SerializeField, Range(0.1f,0.3f)] private float pullBreakDistance; // Maximum distance the pickup can move before being destroyed. Actually starts breaking at 0.95 of this.
    [SerializeField, Range(0.1f, 0.8f)] private float pullBreakMultiplier;
    private Vector2 pullDirection;

    [SerializeField, Range(1, 10)] private int dragMoveSpeed; // How fast the object lerps towards the target position. Use lower values for heavier objects.

    private float pullMoveResistance; // Scales how much resistance the object is to moveing with health, higher health harder to move.
    private float pullBreakTime; // How long the object must be at breaking distance before taking damage, scales with pull break time multiplier
    [SerializeField, Range(0, 0.5f)] private float pullBreakTimeMultiplier; // How long it takes to break multiplied by health
    [SerializeField, Range(0, 6)] private int forceHealthScaling;
    private bool isTryingToBreak; // If the object is currently trying to break. Waits to achive pullBreakTime before breaking.
    private bool isMaxTravel; // If the object has reached maxium movement. Prevents funky movement past boundries.

    private float vibrationIntensity; // Scales between 0 and 1 depending on how close the object is to break distance.
    [SerializeField, Range(0,0.2f)] float vibrationMax; // The maximum movement allowed when vibrating.
    [SerializeField, Range(5, 10)] int vibrationSpeed; // How fast the sprite lerps when vibrating, lower values give a heavier look.
    
    [SerializeField, Range(0,60)] private int respawnCooldown; // How many minutes until respawn after pulling. If 0 then cannot respawn
    private bool isActive; // Whether the object is able to be interacted with.
    private float respawnTime; // Stores the time that the respawn timer will trigger.
    [SerializeField] LayerMask respawnBlockLayerMask; // Which collision layer can block respawns, normaly player and furniture.

    

    // When health == int change pickup's sprite to the one in the dictionary.
    // Sprite 0 is the sprite which will be moved, Sprite 1 is the base which stays still. If the final pull has a base then it will stay
    [SerializeField] private SerializableDictionary<int, Sprite[]> damageDisplayedSprites;

    // An array of dictionaries that contains the items that the pickup will add to inventory taking damage.
    // Place in array - Order which the items are added after taking damage.
    // Item name - Item to be dropped upon destroying the pickup
    // Vector2 - Min,Max. The minimum and maximum amount that can be dropped of the item. Leaving the max as 0 will make the min number the only outcome.
    [SerializeField] private SerializableDictionary<InventoryController.ItemNames, Vector2Int>[] pickupBreakItems;

    private bool isFirstBreak = true;
    [SerializeField, Range(0,20)] private int treeExp;

    [SerializeField] private AudioClip pullingClip;
    private AudioSource specialSource; //Hold this and stop the sound when pulling is done

    // 0, means always active
    [SerializeField, Range(0, 12)] private int pickupOutlineDisplayRange;

    [SerializeField] private Material inRangeMaterial;
    [SerializeField] private Material outRangeMaterial;
    #endregion



    #region Functions

    #region Initialization
    private void Awake()
    {
        spriteArmRef = transform.GetChild(0).gameObject;
        spriteRef = spriteArmRef.transform.GetChild(0).gameObject;
    }

    void Start()
    {
        // Sets the pickup's initial location. This is used as an anchor for sprite arm.
        startPosition = transform.position;
        ToggleObject(false);
        // Ensures variables are set up
        Respawn(); 
    }
    #endregion

    #region Update - Movement, damage, respawn
    // Update is called once per frame
    void Update()
    {
        if (isBeingPulled)
        {
            mouseCurrentPosition = CameraController.Instance.MouseWorldPosition;
            
            pullDirection = (mouseCurrentPosition - mouseStartPosition).normalized;


            // The position that the sprite arm will be trying to move to
            targetPosition = startPosition + ((mouseCurrentPosition - mouseStartPosition) / pullMoveResistance);

            // Slows movement of sprite arm as it gets closer to breaking to look like it's resisting
            float dragMoveSpeedSlowdown = Mathf.Clamp(Mathf.Abs(PullCurrentDistance()-pullBreakDistance)/pullBreakDistance,0.01f,1.0f);

            // If the pickup is trying to break stops movement to prevent weird movement
            if (!isMaxTravel) 
            {
                if (Vector2.Distance(spriteArmRef.transform.position, startPosition) < Vector2.Distance(targetPosition, startPosition))
                {
                    spriteArmRef.transform.position = Vector2.Lerp(spriteArmRef.transform.position, targetPosition, dragMoveSpeed * dragMoveSpeedSlowdown * Time.deltaTime);
                }
                else spriteArmRef.transform.position = Vector2.Lerp(spriteArmRef.transform.position, targetPosition, dragMoveSpeed * Time.deltaTime);


                // Closer the sprite is to breaking vibrate with extra intensity

                vibrationIntensity = (PullCurrentDistance() / pullBreakDistance) * vibrationMax;
                
            }

            // Uses the vibration intensity to create a random vector 2 that the sprite will use to vibrate.
            Vector2 _vibrationOffset = new Vector2(UnityEngine.Random.Range(-vibrationIntensity, vibrationIntensity), UnityEngine.Random.Range(-vibrationIntensity, vibrationIntensity));

            Quaternion _rotateVibration = new Quaternion();
            if (isTryingToBreak) _rotateVibration = Quaternion.Euler(Vector3.forward * (UnityEngine.Random.Range(-vibrationIntensity, vibrationIntensity) * 100));

            // Vibrates sprite independent of sprite arm to prevent funky math
            spriteRef.transform.localPosition = Vector2.Lerp(spriteRef.transform.localPosition, _vibrationOffset, vibrationSpeed * Time.deltaTime);
            spriteRef.transform.localRotation = Quaternion.Lerp(spriteRef.transform.localRotation, _rotateVibration, vibrationSpeed * Time.deltaTime);


            //Play audio effect!
            if (PullCurrentDistance() > 0.025f)
            {
                //While it is breaking play this sound effect, looping.
                specialSource = AudioController.Instance.PlayLoopingSound(pullingClip);
            } else
            {
                if (specialSource != null)
                AudioController.Instance.StopLoopingSound(specialSource);
            }

            // If sprite arm is past break distance then start trying to break the pickup
            if (PullCurrentDistance() > pullBreakDistance * pullBreakMultiplier)
            {



                if (PullCurrentDistance() > pullBreakDistance) isMaxTravel = true;

                // Checks if a break attmempt has started yet
                if (pullBreakTime < Time.time && !isTryingToBreak)
                {
                    // Times how long until the object breaks, time until break scales with health remaining
                    if (forceHealthScaling == 0) pullBreakTime = Time.time + (pullBreakTimeMultiplier * health);
                    else pullBreakTime = Time.time + (pullBreakTimeMultiplier * forceHealthScaling);
                    
                    isTryingToBreak = true;
                }
                // A successful break attempt
                else if (pullBreakTime < Time.time && isTryingToBreak)
                {
                    Mathf.Clamp(health--, 0, 6);
                    // If on 0 health destroys the pickup object, adds resources to inventory, and gives direction for the particle system
                    if (health == 0) EndPull(); 
                    else DamagePull();
                }
            }
            // If player have moved their mouse within the break distance then cancel the break attempt
            if (Vector2.Distance(startPosition, targetPosition) < pullBreakDistance * (pullBreakMultiplier * 0.9))
            {
                isMaxTravel = false;
                isTryingToBreak = false;
            }

            DecorationController.Instance.DecorationSelector.GetComponent<DecorationSelector>().PickupPullingSelectorOffset(mouseStartPosition,  Mathf.Clamp( PullCurrentDistance() / pullBreakDistance, 0.0f, 0.9f), isTryingToBreak);
        }
        else if (!isBeingPulled && isActive) spriteArmRef.transform.position = Vector2.Lerp(spriteArmRef.transform.position, targetPosition, dragMoveSpeed * Time.deltaTime);
        // If the is able to respawn then wait until the correct time then respawn
        else if (Time.time > respawnTime && !isActive && respawnCooldown != 0) Respawn();

        if (Vector2.Distance(transform.position, PlayerController.Instance.transform.position) < pickupOutlineDisplayRange || pickupOutlineDisplayRange == 0)
        {
            spriteRef.GetComponent<SpriteRenderer>().material = inRangeMaterial;
            GetComponent<BoxCollider2D>().enabled = true;
        }
        else
        {
            spriteRef.GetComponent<SpriteRenderer>().material = outRangeMaterial;
            GetComponent<BoxCollider2D>().enabled = false;
        }
            
    }
    #endregion

    #region Pull functions
    // Called by decoration controller when the decoration selector interacts with the pickup
    public void StartPull()
    {
        mouseStartPosition = CameraController.Instance.MouseWorldPosition;
        ResetSpriteVibration();
        if (forceHealthScaling == 0)
            pullMoveResistance =
                Mathf.Clamp(health, 1, 3) * 10; // As the player breaks the object it gets easier to damage
        else pullMoveResistance = forceHealthScaling * 10;
        SetPullBreakState(true, false);
        DecorationController.Instance.DecorationSelector.GetComponent<DecorationSelector>().StartPullingPickup();
    }

    // Called by decoration controller when the player releases the mouse button. Also called when removing health from pickup that doesn't break.
    public void CancelPull()
    {
        AudioController.Instance.StopLoopingSound(specialSource);
        isBeingPulled = false;
        ResetSpriteVibration();
        isTryingToBreak = false;
        DecorationController.Instance.PickupCancel();
        DecorationController.Instance.DecorationSelector.GetComponent<DecorationSelector>().EndPullingPickup();
    }

    public void DamagePull()
    {
        AudioController.Instance.StopLoopingSound(specialSource); //Cancel audio loop

        DamageAddItems(false);
        DamageSetSprites();
        spriteArmRef.transform.position = startPosition;
        SetPullBreakState(false, false);
        ResetSpriteVibration();
        DecorationController.Instance.DecorationSelector.GetComponent<DecorationSelector>().EndPullingPickup();

        
    }

    // Called when pickup has been broken
    public void EndPull() 
    {
        AudioController.Instance.StopLoopingSound(specialSource);
        DamageSetSprites();
        // Set respawn time to a number of minues equal to respawn cooldown.
        respawnTime = Time.time + (respawnCooldown * 60);

        DamageAddItems(true);
        
        ToggleObject(false);
        SetPullBreakState(false, false);
        targetPosition = startPosition;
        DamageSetSprites();
        if (isFirstBreak && treeExp > 0)
        {
            TreeLevelController.Instance.AddCleanExp(treeExp);
            isFirstBreak = false;
        }
        DecorationController.Instance.DecorationSelector.GetComponent<DecorationSelector>().EndPullingPickup();
        
    }
    #endregion

    #region Respawn and toggle
    // Called when respawn timer reaches the correct time and the pickup is able to respawn.
    private void Respawn()
    {
        bool _canRespawn = true;

        //Checks if the player or any furniture is not blocking it's respawn
        Vector2 _hitPosition = GetComponent<BoxCollider2D>().transform.position;
        Vector2 _hitSize = GetComponent<BoxCollider2D>().size;
        float _hitRotation = GetComponent<BoxCollider2D>().transform.rotation.eulerAngles.z;
        if (Physics2D.OverlapBox(_hitPosition, _hitSize, _hitRotation, respawnBlockLayerMask)) _canRespawn = false;

        // Checks that if the pickup has a blocking collider then the player or any furniture is not blocking it's respawn
        if (blockingCollider)
        {
            Vector2 _blockPosition = blockingCollider.GetComponent<BoxCollider2D>().transform.position;
            Vector2 _blockSize = blockingCollider.GetComponent<BoxCollider2D>().size;
            float _blockRotation = blockingCollider.GetComponent<BoxCollider2D>().transform.rotation.eulerAngles.z;
            if (Physics2D.OverlapBox(_blockPosition, _blockSize, _blockRotation, respawnBlockLayerMask)) _canRespawn = false;
        }
        if (_canRespawn)
        {
            spriteArmRef.transform.position = startPosition;
            ResetSpriteVibration();
            mouseStartPosition = Vector2.zero;
            health = healthMax;
            

            GetComponent<SpriteRenderer>().sprite = spriteBase;
            spriteRef.GetComponent<SpriteRenderer>().sprite = sprite;

            ToggleObject(true);
        }
        // If the pickup can't respawn then reset it's respawn timer
        else respawnTime = Time.time + (respawnCooldown * 60);

    }

    // Called when the pickup is pulled or respawned. Makes it appear as if it has been destroyed/respawned.
    private void ToggleObject(bool _toggle)
    {
        isActive = _toggle;
        if (_toggle)
        {
            spriteRef.GetComponent<SpriteRenderer>().color = Color.white;
        }
        else
        {
            spriteRef.GetComponent<SpriteRenderer>().color = Color.clear;
            if (healthMax > 1 && damageDisplayedSprites.ContainsKey(1)) GetComponent<SpriteRenderer>().sprite = damageDisplayedSprites[1][1];
        } 
            
        GetComponent<BoxCollider2D>().enabled = _toggle;
        if (blockingCollider) blockingCollider.GetComponent<BoxCollider2D>().enabled = _toggle;
    }
    #endregion


    #region Utility Functions
    // Returns distance of sprite from start location
    private float PullCurrentDistance()
    {
        return Vector2.Distance(startPosition, spriteArmRef.transform.position);
    }

    // Tells the inventory contoller what items were dropped by the pickup. Informs the controller if the pickup was damaged or broken.
    private void DamageAddItems(bool _broken) 
    { 
        if (_broken) DecorationController.Instance.PickupBroken(pickupBreakItems[healthMax - 1], spriteArmRef.transform.position);
        else DecorationController.Instance.PickupDamaged(pickupBreakItems[healthMax - (health + 1)], spriteArmRef.transform.position);
    }

    private void DamageSetSprites()
    {
        // Change sprites depending on health remaining
        if (damageDisplayedSprites.ContainsKey(health))
        {
            spriteRef.GetComponent<SpriteRenderer>().sprite = damageDisplayedSprites[health][0];
            GetComponent<SpriteRenderer>().sprite = damageDisplayedSprites[health][1];
        }
    }

    private void SetPullBreakState(bool _pull, bool _break)
    {
        isBeingPulled = _pull;
        isTryingToBreak = _break;
    }

    private void ResetSpriteVibration()
    {
        spriteRef.transform.localPosition = Vector2.zero;
        spriteRef.transform.localRotation = new Quaternion();
        targetPosition = startPosition;
    }
    #endregion

    #region Context Menu Functions

    // Sets up sprites of the pickup object using the serialized sprite variables 
    [ContextMenu ("Set Sprite")]
    private void SetupSprite()
    {
        spriteArmRef = transform.GetChild(0).gameObject;
        spriteRef = spriteArmRef.transform.GetChild(0).gameObject;
        GetComponent<SpriteRenderer>().sprite = spriteBase;
        spriteRef.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    // Setup sprite and item drop dictionaries using health max for the values
    [ContextMenu ("Initialize damage arrays")] 
    private void SetupDamageArrays()
    {
        // Add a number of item dictionaries equal to health max. Allows for unique drops for each damage level, but doesn't have to.
        pickupBreakItems = new SerializableDictionary<InventoryController.ItemNames, Vector2Int>[healthMax];

        // Add a number of dictonary elements equal to health -1.
        damageDisplayedSprites = new SerializableDictionary<int, Sprite[]>();
        if (healthMax > 1)
        {
            for (int i = 1; 0 < healthMax - i; i++)
            {
                damageDisplayedSprites.Add(healthMax - i, new Sprite[2]);
            }
        }

        
    }
    #endregion

    #endregion

}
