using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DecorationSelector : MonoBehaviour
{
    #region Private Variables

    // Assets and components
    private Sprite sprite;
    [SerializeField] private Sprite[] selectorSpritesArray; // 0-empty, 1-pickable, 2-pickupPlaceable, 3-Bad, 4-Offgrid
    private SpriteRenderer spriteRenderer;
    
    private enum SelectorState {EMPTY, PICKABLE, PICKUP_PLACEABLE, BAD, OFFGRID } // All states that the selector can be in
    private SelectorState selectorState = SelectorState.EMPTY; // The selector's current state. EMPTY is the defult state it will be in
        
    private Vector3 selectorTargetLocation; // Holds mouse location
    [SerializeField] float selectorMoveSpeed; // The speed that the selector follows the mouse.
    
    private float selectorSpinSpeed; // The current speed that the selector will spin, from selectorSpinSpeedArray based on selector state
    [SerializeField] private float[] selectorSpinSpeedArray; //0-empty(stop), 1-pickable(slow), 2-pickupPlaceable(normal), 3-Bad(fast), 4-Offgrid(normal)
    
    [SerializeField] private float scaleSpeed; // The speed that the scale lerps when changing
    [SerializeField] private Vector3 baseScaleValue; // The base scale that the selector has, will be worked out using the scale of the tilemap
    private Vector3 targetScaleValue;
    private float scaleMultiplier; // The target scale that the selector will lerp to, from selectorScaleMultiplierArray based on selector state
    [SerializeField] private float[] selectorScaleMultiplierArray; //0-empty(normal), 1-pickable(big), 2-pickupPlaceable(big), 3-Bad(small), 4-Offgrid(normal)
    
    

    [SerializeField] private AnimationCurve jumpCurve; // The curve the selector will follow when jumping afer a click, gives an elastic look
    private float scaleJump = 1.0f; // The ammount that scale is affected when jump
    private float scaleJumpTimer = 1.0f; // Time since the last click
    [SerializeField] private float scaleJumpMultiplier; // Multiplies scale jump by this value
    private float spinJump = 1.0f; // The ammount that spin is affected when jump
    private float spinJumpTimer = 1.0f;// Time since the last click
    [SerializeField] private float spinJumpMultiplier; // Multiplies spin jump by this value
    private float mouseDownSlowDown = 1f; // Multiplies rotation and scales by this, only changed when holding down a click
    
    

    
    [SerializeField] private LayerMask selectorInteractionLayerMask; // Collision layers that selector interacts with
    private Collider2D mouseDownObjectHit; // Holds the object that was selected on a click
    private TrashObject mouseDownHeldTrash;

    #endregion

    #region Functions
    
    #region Initialisation
    private void Awake()
    {
        sprite = GetComponent<Sprite>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
    }
    #endregion
    
    private void Update()
    {
        // Manages the spin jump of the selector when clicking, ensuring it progresses smoothly makes the selector return to normal after
        #region Selector scale and spin jump management

        scaleJumpTimer += Time.deltaTime;
        spinJumpTimer += Time.deltaTime;
        if (scaleJumpTimer < 0.5f)
        {
            scaleJump = jumpCurve.Evaluate(spinJumpTimer) * scaleJumpMultiplier;
        }
        else
        {
            scaleJump = 1;
        }
        if (spinJumpTimer < 0.5f)
        {
            spinJump = jumpCurve.Evaluate(spinJumpTimer) * spinJumpMultiplier;
        }
        else
        {
            spinJump = 1;
        }

        #endregion

        // Controls the selector's current position, scale and rotation. Scale and rotation are affected by the selector's state, and spin jump when clicking
        #region Selector transform control

        selectorTargetLocation = GetMousePositionInWorld(transform.position.z); // Sets target location as mouse position
        transform.position = Vector3.Lerp(transform.position, selectorTargetLocation, selectorMoveSpeed * Time.deltaTime); // The selector will lerp towards the mouse position
        targetScaleValue = Vector3.Scale(baseScaleValue, baseScaleValue * scaleMultiplier * scaleJump) * mouseDownSlowDown; // Gets the target scale of the selector, affected by changing state or when spin jumping
        transform.localScale = Vector3.Lerp(transform.localScale, targetScaleValue, scaleSpeed * mouseDownSlowDown * Time.deltaTime); // Lerps scale of selector towards target scale
        transform.rotation = Quaternion.Lerp(transform.rotation, transform.rotation * Quaternion.AngleAxis(90, Vector3.forward), selectorSpinSpeed * mouseDownSlowDown * Time.deltaTime * spinJump); // The selector is always rotating, the speed of this rotation is affected by changing state or when spin jumping
        
        #endregion
        
        // Controls the visual and funcionality of mouse clicks, object iteraction is passed through to Decoration Controller rather than being handled here
        #region Click control

        if (Input.GetMouseButtonDown(0))
        {
            mouseDownSlowDown = 0.9f; // Slows to rotation and reduced the scale of the selector slightly while holding a click
            if (CheckObjectUnderMouse()) // If an interactable object is below the mouse
            {
                mouseDownObjectHit = CheckObjectUnderMouse(); // Stores the object that was under the mouse
                DecorationController.Instance.SelectorDecorationObjectInteract(mouseDownObjectHit.gameObject, true);
                if (CheckObjectUnderMouse().GetComponent<TrashObject>()) mouseDownHeldTrash = CheckObjectUnderMouse().GetComponent<TrashObject>();
                else mouseDownHeldTrash = null;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            scaleJumpTimer = 0.0f; // Makes selector scale jump with an elastic effect
            spinJumpTimer = 0.0f; // Makes selector rotation speed jump with an elastic effect
            mouseDownSlowDown = 1f; // Resets rotation and scale to normal

            if (mouseDownHeldTrash) mouseDownHeldTrash.CancelPull();
            mouseDownHeldTrash = null;

            if (CheckObjectUnderMouse() == mouseDownObjectHit) // Checks if the click is released over the same object that it began on
            {
                DecorationController.Instance.SelectorDecorationObjectInteract(mouseDownObjectHit.gameObject, false); // Signals Decoration Controller that the object has been interacted with
            }
        }

        #endregion


        // This section of code controls the selector switching between states and visuals depending on what the player is currently doing
        #region SelectorVisualStateSwitching

        if (DecorationController.Instance.CurrentMoveFake)
        {
            if (DecorationController.Instance.CurrentMoveFake.GetComponent<DecorationMovingFake>().CheckIfPlaceable())
            {
                selectorState = SelectorState.PICKUP_PLACEABLE;
            }
            else
            {
                selectorState = SelectorState.BAD;
            }
        }
        else if (mouseDownHeldTrash != null) selectorState = SelectorState.OFFGRID;
        else
        {
            Collider2D _objectUnderMouse = CheckObjectUnderMouse();
            if (_objectUnderMouse)
            {
                selectorState = SelectorState.PICKABLE;
                if (_objectUnderMouse.GetComponent<DecorationObject>()) { _objectUnderMouse.GetComponent<DecorationObject>().StartHover(); }
                else if (_objectUnderMouse.GetComponent<DecorationButton>()) { _objectUnderMouse.GetComponentInParent<DecorationObject>().StartHover(); }
            }
            else
            {
                selectorState = SelectorState.EMPTY;
            }
        }
        int state = (int)selectorState; // Casts selector state into an int. (Scrapes the int value from it)
        spriteRenderer.sprite = selectorSpritesArray[state];
        selectorSpinSpeed = selectorSpinSpeedArray[state];
        scaleMultiplier = selectorScaleMultiplierArray[state];

        #endregion 
        
        
    }

    public Vector3 GetMousePositionInWorld(float _zValue)
    {
        return new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, _zValue);
    }

    public Collider2D CheckObjectUnderMouse()
    {
        return Physics2D.OverlapCircle(GetMousePositionInWorld(transform.position.z), 0.1f, selectorInteractionLayerMask);
        
    }
    
    #endregion


}