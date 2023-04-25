using UnityEngine;

public class WallJump : MonoBehaviour
{

    private SpriteRenderer[] sprites;
    
    private void Start()
    {
        TreeLevelController.Instance.OnTreeLevelUp += treeLevelUpUpdate;
        sprites = GetComponentsInChildren<SpriteRenderer>();
    }

    private void treeLevelUpUpdate()
    {
        if (TreeLevelController.Instance.CurrentTreeLevel == 2) 
        {
            foreach (SpriteRenderer sprite in sprites)
            {
                sprite.color = Color.white;
            }
            GetComponent<BoxCollider2D>().enabled = true;
        }
    }
}
