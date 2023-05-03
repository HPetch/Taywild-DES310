using UnityEngine;

[ExecuteAlways]
public class DecorationButton : MonoBehaviour
{
    public enum ButtonType { PICKUP, STYLE_LEFT, STYLE_RIGHT, STYLE_HOLDER}
    [field: SerializeField] public ButtonType buttonType { get; private set; }

    private float baseHolderSpacing = 0.4f;
    [field: SerializeField, Range(0,1)] public float editHolderSpacing { get; private set; }


    void Update()
    {
        
        if (buttonType == ButtonType.STYLE_LEFT || buttonType == ButtonType.STYLE_RIGHT)
        {
            editHolderSpacing = transform.parent.GetComponent<DecorationButton>().editHolderSpacing;
            if (buttonType == ButtonType.STYLE_LEFT) { transform.position = new Vector2(transform.parent.position.x - baseHolderSpacing - editHolderSpacing, transform.position.y); }
            else if (buttonType == ButtonType.STYLE_RIGHT) { transform.position = new Vector2(transform.parent.position.x + baseHolderSpacing + editHolderSpacing, transform.position.y); }
        }

    }
}


