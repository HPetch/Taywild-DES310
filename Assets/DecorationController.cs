using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationController : MonoBehaviour
{
    public static DecorationController Instance { get; private set; }
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
}
