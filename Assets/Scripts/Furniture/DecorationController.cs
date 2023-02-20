using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationController : MonoBehaviour
{
    public static DecorationController Instance { get; private set; }
    [SerializeField] private GameObject decorationSelectorPrefab;
    [SerializeField] private GameObject decorationMovingFakePrefab;
    public GameObject CurrentMoveTarget { get; private set; }
    public GameObject CurrentMoveFake { get; private set; }
    [field: SerializeField] public GameObject DecorationSelector { get; private set; }
// Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!CurrentMoveFake) // If no decoration is being moved, normal interaction mode
        {
            
        }
        else // If a decoration is being moved, cursor shouldn't interact with anything else
        {
            if (CurrentMoveFake.GetComponent<DecorationMovingFake>().CheckIfPlaceable()) // If the movement fake is in a viable placement location
            {
                if(Input.GetMouseButtonUp(0))
                {
                    DecorationMoveEndStart(); // Delete the movement fake and place the actual decoration in it's place
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                DecorationMoveCancel();
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                CurrentMoveFake.GetComponent<DecorationMovingFake>().scrollRotate(Input.GetAxis("Mouse ScrollWheel") > 0f);
            }
        }
    }


    public void SelectorDecorationObjectInteract(GameObject _selectedObject)
    {
        if (_selectedObject.GetComponent<DecorationObject>() && _selectedObject != CurrentMoveTarget )
        {
            DecorationMoveStart(_selectedObject);
        }
    }
    
    public void DecorationMoveStart(GameObject _decorationObject)
    {
        CurrentMoveTarget = _decorationObject;
        CurrentMoveFake = Instantiate(decorationMovingFakePrefab);
        CurrentMoveTarget.GetComponent<SpriteRenderer>().color = new Color(1,1,1,0.25f);
        CurrentMoveFake.GetComponent<DecorationMovingFake>().scrollRotateIndex = CurrentMoveTarget.GetComponent<DecorationObject>().scrollRotateArrayHolder;
    }

    private void DecorationMoveEndStart()
    {
        CurrentMoveFake.GetComponent<DecorationMovingFake>().placeDecorationFake();
    }
    public void DecorationMoveEndFinish()
    {
        CurrentMoveTarget.transform.position = CurrentMoveFake.transform.position;
        CurrentMoveTarget.transform.rotation = CurrentMoveFake.transform.rotation;
        CurrentMoveTarget.GetComponent<DecorationObject>().scrollRotateArrayHolder = CurrentMoveFake.GetComponent<DecorationMovingFake>().scrollRotateIndex;
        Destroy(CurrentMoveFake);
        CurrentMoveFake = null;
        CurrentMoveTarget.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
        CurrentMoveTarget = null;
    }

    private void DecorationMoveCancel()
    {
        Destroy(CurrentMoveFake);
        CurrentMoveFake = null;
        CurrentMoveTarget.GetComponent<SpriteRenderer>().color = new Color(1,1,1,1);
        CurrentMoveTarget = null;
    }

    // Event: Enter edit mode
    // Instantiate decoration selector
}
