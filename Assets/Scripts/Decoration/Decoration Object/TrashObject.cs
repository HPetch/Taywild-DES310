using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TrashObject : MonoBehaviour
{
    public event Action<TrashObject> OnStartPullTrash;
    public event Action<string, Vector2> OnEndPullTrash; // String will be replaced with a list that contains the recourses the trash contains for inventory controller. Vector 2 will contain the direction it was broken in. For a particle system to throw plant bits in that direction.

    //[SerializeField] private List<InventoryController.Instance.CraftResource, int>;

    private Vector2 startPosition;
    private Vector2 targetPosition;
    

    private Vector2 mouseStartPosition;
    private Vector2 mouseCurrentPosition;
    private Vector2 mouseWorldPosition;
    private Vector2 mouseVisualOffset;

    private bool isBeingPulled;
    [SerializeField] private float pullMaxDistance;
    private float pullCurrentDistance;

    [SerializeField, Range(1,10)] private float visualPullMovementAllowed = 10;

    private float vibrationIntensity;
    [SerializeField, Range(0,1)] float vibrationMax;

    [SerializeField] private int health;

    ///
    /// Need to get inventory system and have the trash know what resources it contains, possibly slightly randomised.
    /// The inventory controller would listen for the OnEndPullTrash event and collect the resources from it.
    /// 



    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
        ///
        /// Will sort out what resources the trash object has here
        /// Will have a common resource which is always dropped with variance in it's amount
        /// Will have a rare resource which drops in low amounts, smaller trash might not have a rare resource
        /// i.e. A blight bramble would have common resource of plant fibre (3-6 drops) and a rare resource of thorn (1-2) drops
        /// A small fallen branch: Common - Wood(2-4, Rare - Leaf(1-3)
        /// A large fallen branch: Common - Wood(6-8), Rare - Leaf(0-2)
        /// Horned moss: Common - Plant fibre(1-3)
        ///
    }

    // Update is called once per frame
    void Update()
    {
        if (isBeingPulled)
        {
            mouseCurrentPosition = Input.mousePosition;
            pullCurrentDistance = Vector2.Distance(mouseStartPosition, mouseCurrentPosition);
            if (pullCurrentDistance > pullMaxDistance)
            {
                health--;
                if (health == 0) EndPull((mouseCurrentPosition-mouseStartPosition).normalized); ; //Destroys the trash object, adds resources to inventory, and gives direction for the particle system
                
            }
            else
            {
                // Closer mouse is to reaching pullMaxDistance vibrate the sprite
                vibrationIntensity = pullCurrentDistance / pullMaxDistance;
                float _vibrationAmount = (vibrationMax/10) * vibrationIntensity;
                Vector2 _vibrationOffset = new Vector2(UnityEngine.Random.Range(-_vibrationAmount, _vibrationAmount), UnityEngine.Random.Range(-_vibrationAmount, _vibrationAmount));
                
                mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                targetPosition = startPosition + ((mouseCurrentPosition - mouseStartPosition) / (visualPullMovementAllowed * 100));

                targetPosition = targetPosition += _vibrationOffset;
                
                transform.position = Vector2.Lerp(transform.position, targetPosition, 2);
            }
        }
        else transform.position = startPosition;
            
    }

    public void StartPull()
    {
        mouseStartPosition = Input.mousePosition;
        OnStartPullTrash?.Invoke(this);
        isBeingPulled = true;
    }

    public void EndPull(Vector2 _direction) 
    { 
        isBeingPulled = false;
        OnEndPullTrash?.Invoke("List will go here instead :)", _direction);
        Destroy(this.gameObject);
    }
    
}
