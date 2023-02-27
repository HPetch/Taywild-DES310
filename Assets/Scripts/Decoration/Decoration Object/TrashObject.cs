using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashObject : MonoBehaviour
{
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

    // Start is called before the first frame update
    void Start()
    {
        startPosition = transform.position;
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
                if (health == 0) Destroy(this.gameObject); //Add resources to inventory
                EndPull();
            }
            else
            {
                // Closer mouse is to reaching pullMaxDistance vibrate the sprite
                vibrationIntensity = pullCurrentDistance / pullMaxDistance;
                float _vibrationAmount = (vibrationMax/10) * vibrationIntensity;
                Vector2 _vibrationOffset = new Vector2(Random.Range(-_vibrationAmount, _vibrationAmount), Random.Range(-_vibrationAmount, _vibrationAmount));

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
        isBeingPulled = true;
    }

    public void EndPull() { isBeingPulled = false; }
    
}
