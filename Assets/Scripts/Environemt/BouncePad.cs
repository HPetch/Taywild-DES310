using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncePad : MonoBehaviour
{
    [SerializeField] private float bounceFactor = 20f;
    [SerializeField] private AudioClip[] bounceSounds;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //Apply a big boost upwards.
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * bounceFactor, ForceMode2D.Impulse);

            //Add sound
            AudioClip clip = bounceSounds[Random.Range(0, bounceSounds.Length - 1)];
            AudioController.Instance.PlaySound(clip, false);
        }
    }
}
