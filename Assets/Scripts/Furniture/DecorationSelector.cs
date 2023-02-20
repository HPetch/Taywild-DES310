using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DecorationSelector : MonoBehaviour
{
    #region Public Variables
    #endregion
    #region Protected Variables
    #endregion
    #region Private Variables
    private Sprite sprite;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] selectorSpritesArray; // 0-empty, 1-pickable, 2-pickupPlaceable, 3-Bad, 4-Offgrid
    private Vector3 selectorTargetLocation;
    [SerializeField] float selectorMoveSpeed; // The speed that the selector lerps while following the mouse.
    private float selectorSpinSpeed; // The current speed that the selector will spin
    [SerializeField] private float[] selectorSpinSpeedArray; //0-empty(stop), 1-pickable(slow), 2-pickupPlaceable(normal), 3-Bad(fast), 4-Offgrid(normal)
    [SerializeField] private float scaleSpeed; // The speed that the scale lerps when changing
    [SerializeField] private Vector3 baseScaleValue; // The base scale that the selector has, will be worked out using the scale of the tilemap
    private Vector3 targetScaleValue;
    private float scaleMultiplier;
    [SerializeField] private float[] selectorScaleMultiplierArray; //0-empty(normal), 1-pickable(big), 2-pickupPlaceable(big), 3-Bad(small), 4-Offgrid(normal)
    private enum SelectorState {EMPTY, PICKABLE, PICKUP_PLACEABLE, BAD, OFFGRID }
    private SelectorState selectorState = SelectorState.EMPTY;
    private int selectorLayerMask;

    [SerializeField] private AnimationCurve jumpCurve;
    private float scaleJump = 1;
    private float scaleJumpTimer = 1;
    [SerializeField] private float scaleJumpMultiplier;
    private float spinJump = 1;
    private float spinJumpTimer;
    [SerializeField] private float spinJumpMultiplier;

    private bool isHoldingFurniture;
    private bool isHoveringOverEditableObject;

    private float mouseDownSlowDown = 1f;

    #endregion

    #region Functions
    #region Initialisation
    private void Awake()
    {
        sprite = GetComponent<Sprite>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        selectorLayerMask = LayerMask.GetMask("Furniture");
    }
    private void Start()
    {
    }
    #endregion

    // Update is called once per frame
    private void Update()
    {
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



        selectorTargetLocation = GetMousePositionInWorld(transform.position.z);
        transform.position = Vector3.Lerp(transform.position, selectorTargetLocation, selectorMoveSpeed * Time.deltaTime);
        targetScaleValue = Vector3.Scale(baseScaleValue, baseScaleValue * scaleMultiplier * scaleJump) * mouseDownSlowDown;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScaleValue, scaleSpeed * mouseDownSlowDown * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, transform.rotation * Quaternion.AngleAxis(90, Vector3.forward), selectorSpinSpeed * mouseDownSlowDown * Time.deltaTime * spinJump);
        CheckObjectUnderMouse();

        if (Input.GetMouseButtonDown(0))
        {
            mouseDownSlowDown = 0.9f;
        }
        if (Input.GetMouseButtonUp(0))
        {
            scaleJumpTimer = 0.0f;
            spinJumpTimer = 0.0f;
            mouseDownSlowDown = 1f;
            if (selectorState == SelectorState.PICKABLE)
            {
                if (CheckObjectUnderMouse().gameObject.GetComponent<DecorationObject>())
                {
                    DecorationController.Instance.DecorationMoveStart(CheckObjectUnderMouse().gameObject);
                }
                
            }
        }
    }

    public Vector3 GetMousePositionInWorld(float _zValue)
    {
        return new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y, _zValue);
    }

    public Collider2D CheckObjectUnderMouse()
    {
        Collider2D hit = Physics2D.OverlapCircle(GetMousePositionInWorld(transform.position.z), 0.1f, selectorLayerMask);
        if (hit)
        {
            selectorState = SelectorState.PICKABLE;
        }
        else
        {
            selectorState = SelectorState.EMPTY;
        }
        
        int state = (int)selectorState; // Casts selector state into an int. (Scrapes the int value from it)
        spriteRenderer.sprite = selectorSpritesArray[state];
        selectorSpinSpeed = selectorSpinSpeedArray[state];
        scaleMultiplier = selectorScaleMultiplierArray[state];
        return hit;
    }




    public void DestroySelector()
    {

        // if holding furnature put it down
        Destroy(this.gameObject);
    }
    #endregion


}