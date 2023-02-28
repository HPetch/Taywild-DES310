using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleEffectAutoDestroy : MonoBehaviour
{
    private void Awake()
    {
        StartCoroutine(SelfDestruct());
    }

    private IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(GetComponent<ParticleSystem>().main.duration);
        Destroy(gameObject);
    }    
}