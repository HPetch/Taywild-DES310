using UnityEngine;

public class Clutter : MonoBehaviour
{
    [SerializeField] private bool shake = true;

    [Range(0, 2)]
    [SerializeField] private float shakeStrength = 0.1f;

    [Range(0, 1)]
    [SerializeField] private float shakeDuration = 0.2f;

    [Range(0, 2)]
    [SerializeField] private float effectCooldown = 1.0f;
    private float timeOfLastEffect = 0.0f;

    private Vector2 defaultPosition = Vector2.zero;
    private ParticleSystem[] particleEffects = null;

    private void Awake()
    {
        defaultPosition = transform.localPosition;
        particleEffects = GetComponentsInChildren<ParticleSystem>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (Time.time - timeOfLastEffect > effectCooldown)
            {
                timeOfLastEffect = Time.time;

                foreach (ParticleSystem particleEffect in particleEffects)
                {
                    particleEffect.Play();
                }

                if (shake)
                {
                    LeanTween.cancel(gameObject);
                    transform.localPosition = defaultPosition;
                    LeanTween.moveLocal(gameObject, (Vector2)transform.localPosition - new Vector2(shakeStrength, shakeStrength / 5f), shakeDuration).setEaseShake();
                }
            }
        }                
    }
}