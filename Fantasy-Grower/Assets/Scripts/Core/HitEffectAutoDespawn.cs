using System.Collections;
using UnityEngine;

public class HitEffectAutoDespawn : MonoBehaviour
{
    private HitEffectObjPool pool;
    private ParticleSystem particleSystem;
    private Coroutine despawnCoroutine;

    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    public void Play(HitEffectObjPool pool)
    {
        this.pool = pool;

        if (despawnCoroutine != null)
        {
            StopCoroutine(despawnCoroutine);
        }

        particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        particleSystem.Play(false);

        despawnCoroutine = StartCoroutine(WaitForParticleEnd());
    }

    private IEnumerator WaitForParticleEnd()
    {
        yield return null;

        while (particleSystem.IsAlive(false))
        {
            yield return null;
        }

        pool.Despawn(this);
    }
}
