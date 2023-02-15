using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationController : MonoBehaviour
{
    public static DecorationController Instance { get; private set; }
    [SerializeField] private GameObject decorationSelectorPrefab;
    [SerializeField] private GameObject decorationMovingFakePrefab;
    public GameObject CurrentMoveTarget { get; private set; }
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
        
    }

    public void DecorationMoveStart(GameObject _decorationObject)
    {
        CurrentMoveTarget = _decorationObject;
        GameObject decorationFake = Instantiate(decorationMovingFakePrefab);
        decorationFake.GetComponent<DecorationMovingFake>().Inititalise();
        CurrentMoveTarget.GetComponent<SpriteRenderer>().color = Color.grey;
    }

    // Event: Enter edit mode
    // Instantiate decoration selector
}
