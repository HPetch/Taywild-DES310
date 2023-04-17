using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField] private float inactiveBounceFactor = 20f;
    [SerializeField] private float activeBounceFactor = 20f;

    private void Update()
    {
        Debug.DrawLine(transform.position, transform.position + -transform.up, Color.red);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //Apply a big boost upwards.
            PlayerController.Instance.MushroomBounce();
            float _bounceFactor = inactiveBounceFactor;
            if (TreeLevelController.Instance.CurrentTreeLevel >= 1) _bounceFactor = activeBounceFactor;
            //collision.gameObject.GetComponent<Rigidbody2D>().AddForce(-transform.up * _bounceFactor, ForceMode2D.Impulse);
            //Vector2 _transform = new Vector2(-transform.up.x * 20, -transform.up.y);
            collision.gameObject.GetComponent<Rigidbody2D>().velocity = -transform.up * _bounceFactor;
        }
    }
}
