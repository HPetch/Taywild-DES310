using UnityEngine;
using Random = UnityEngine.Random;

public class DecorationPickupDebris : MonoBehaviour
{
    private Vector2 startTargetLocation;
    [SerializeField] private float movementSpeed;
    private float hangTime;
    [SerializeField] private float hangDelay;
    [SerializeField] private Vector2[] finalTargetLocationArray;
    private Vector2 finalTargetLocation;
    private bool isHang = true;
    [SerializeField] private SerializableDictionary<InventoryController.ItemNames, Sprite> spriteDict;


    // Start is called before the first frame update
    public void initialize(InventoryController.ItemNames _item, Vector2 _startLocation, float _startTravelDistance)
    {
        if (spriteDict.ContainsKey(_item))
        {
            GetComponent<SpriteRenderer>().sprite = spriteDict[_item];
        }

        transform.position = _startLocation;
        Vector2 _startRotation = Random.insideUnitCircle.normalized;
        startTargetLocation = _startLocation + (_startRotation * _startTravelDistance);
        hangDelay += Random.Range(hangDelay * -0.5f, hangDelay * 1.5f);

        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (isHang)
        {
            transform.position = Vector2.Lerp(transform.position, startTargetLocation, movementSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, startTargetLocation) < 0.1f)
            {
                hangTime = Time.time + hangDelay;
                isHang = false;
                movementSpeed *= 3;
                
            }
        }
        else if (hangTime < Time.time)
        {
            //// WILL BE REPLACED ONCE CORNER RESOURCE UI IS IN
            finalTargetLocation = PlayerController.Instance.transform.position;
            
            transform.position = Vector2.MoveTowards(transform.position, finalTargetLocation, movementSpeed * Time.deltaTime);
            if (Vector2.Distance(transform.position, finalTargetLocation) < 1f) /* Add numbers to corner */ Destroy(gameObject);
        }
    }
}
