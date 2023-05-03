using UnityEngine;

public class Clutter : MonoBehaviour
{
    [Range(0, 2)]
    [SerializeField] private float shakeEffectStrength = 0.1f;

    [Range(0, 1)]
    [SerializeField] private float shakeEffectDuration = 0.2f;

    [Range(0, 2)]
    [SerializeField] private float shakeEffectCooldown = 1.0f;
    private float timeOfLastShake = 0.0f;

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
            if (Time.time - timeOfLastShake > shakeEffectCooldown)
            {
                timeOfLastShake = Time.time;

                foreach (ParticleSystem particleEffect in particleEffects)
                {
                    particleEffect.Play();
                }

                LeanTween.cancel(gameObject);
                transform.localPosition = defaultPosition;
                LeanTween.moveLocal(gameObject, (Vector2)transform.localPosition - new Vector2(shakeEffectStrength, shakeEffectStrength), shakeEffectDuration).setEaseShake();
            }
        }                
    }
}