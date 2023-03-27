using UnityEngine;

public class DecorationObject : MonoBehaviour
{

    // SHOULD CHANGE TO INHERITANCE. Environment object should be base. Decoration object

    [field: SerializeField] public GameObject RemoveButton { get; protected set; }
    [field: SerializeField] public GameObject EditButtonHolder { get; protected set; }
    public GameObject EditButtonLeft { get; protected set; }
    public GameObject EditButtonRight { get; protected set; }

    protected bool isMoving;
    private bool isHovered;
    private float timeOfLastHover = 0f;


    private void Start()
    {
        EndHover();
        
        if(EditButtonHolder) EditButtonHolder.transform.position = new Vector2(transform.position.x, GetComponent<BoxCollider2D>().bounds.min.y);
    }

    private void Update()
    {
        if ( isHovered && Time.time - timeOfLastHover > 1f || isMoving) EndHover();
    }

    public void StartHover()
    {
        if (!isMoving)
        {
            if (RemoveButton) ShowButton(RemoveButton);
            if (EditButtonLeft) ShowButton(EditButtonLeft);
            if (EditButtonRight) ShowButton(EditButtonRight);
            timeOfLastHover = Time.time;
            isHovered = true;
        }
        else { EndHover(); }
    }

    public void EndHover()
    {
        if (RemoveButton) HideButton(RemoveButton);
        if (EditButtonLeft) HideButton(EditButtonLeft);
        if (EditButtonLeft) HideButton(EditButtonRight);
        isHovered = false;
    }

    


    private void HideButton(GameObject _button)
    {
        if (_button.GetComponent<DecorationButton>())
        {
            _button.GetComponent<SpriteRenderer>().color = Color.clear;
            _button.GetComponent<BoxCollider2D>().enabled = false;
        }

    }
    private void ShowButton(GameObject _button)
    {
        if (_button.GetComponent<DecorationButton>())
        {
            _button.GetComponent<SpriteRenderer>().color = Color.white;
            _button.GetComponent<BoxCollider2D>().enabled = true;
        }
    }

    

    private void Reset()
    {
        

        
    }
}
