using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AirCurrent : MonoBehaviour
{
    public enum AirCurrentDirectionTypes { UP, LEFT, RIGHT }
    [SerializeField] public AirCurrentDirectionTypes AirCurrentDirectionType = AirCurrentDirectionTypes.UP;

    [field: Range(0, 15)]
    [field: SerializeField] public float AirCurrentSpeed { get; private set; } = 10f;

    [field: Range(0, 15)]
    [field: SerializeField] public float AirCurrentFightSpeed { get; private set; } = 3f;

    [SerializeField] private bool displayDebugInfo = true;
    private Rect airCurrentRect;

    private void Awake()
    {
        displayDebugInfo = false;

        transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;

        Vector2 size = GetComponent<BoxCollider2D>().size;
        airCurrentRect = new Rect((Vector2)transform.position - (size / 2), size);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            ParticleSystem particleSystem = GetComponentInChildren<ParticleSystem>();
            particleSystem.transform.localPosition = new Vector2(0, -transform.localScale.y / 2);

            if (particleSystem.main.duration != transform.localScale.y)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ParticleSystem.MainModule mainModule = particleSystem.main;
                mainModule.duration = transform.localScale.y;
                mainModule.startLifetime = transform.localScale.y;
                particleSystem.Play();
            }

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.radius = transform.localScale.x;


            if (displayDebugInfo)
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
                transform.GetChild(0).transform.localScale = GetComponent<BoxCollider2D>().size;
                transform.GetChild(0).transform.SetPositionAndRotation(transform.position, transform.rotation);
            }
            else
            {
                transform.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
            }

            return;
        }
#endif    

        if (airCurrentRect.Contains(PlayerController.Instance.transform.position))
        {
            PlayerController.Instance.AddAirCurrent(this);
        }
        else
        {
            PlayerController.Instance.RemoveAirCurrent(this);
        }
    }
}