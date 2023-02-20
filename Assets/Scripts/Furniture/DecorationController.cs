using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationController : MonoBehaviour
{
    public static DecorationController Instance { get; private set; }
    [SerializeField] private GameObject decorationSelectorPrefab;
    [SerializeField] private GameObject decorationMovingFakePrefab;
    public GameObject CurrentMoveTarget { get; private set; }
    private GameObject currentMoveFake;
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
        if (currentMoveFake)
        {
            if (currentMoveFake.GetComponent<DecorationMovingFake>().CheckIfPlaceable())
            {
                if(Input.GetMouseButtonUp(0))
                {
                    DecorationMoveEnd();
                }
            }
        }
    }

    public void DecorationMoveStart(GameObject _decorationObject)
    {
        CurrentMoveTarget = _decorationObject;
        currentMoveFake = Instantiate(decorationMovingFakePrefab);
        CurrentMoveTarget.GetComponent<SpriteRenderer>().color = Color.grey;
    }

    private void DecorationMoveEnd()
    {
        CurrentMoveTarget.transform.position = currentMoveFake.transform.position;
        Destroy(currentMoveFake);
        currentMoveFake = null;
        CurrentMoveTarget.GetComponent<SpriteRenderer>().color = Color.white;
    }

    // Event: Enter edit mode
    // Instantiate decoration selector
}
