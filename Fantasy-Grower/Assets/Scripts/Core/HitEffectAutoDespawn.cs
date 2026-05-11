using System.Collections;
using UnityEngine;

public class HitEffectAutoDespawn : MonoBehaviour
{
    private HitEffectObjPool pool;
    private ParticleSystem[] particleSystems;
    private Coroutine despawnCoroutine;

    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>(true);
    }

    public void Play(HitEffectObjPool pool)
    {
        this.pool = pool;

        if (despawnCoroutine != null)
        {
            StopCoroutine(despawnCoroutine);
        }

        for (int i = 0; i < particleSystems.Length; i++)
        {
            particleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particleSystems[i].Play(true);
        }

        despawnCoroutine = StartCoroutine(WaitForParticleEnd());
    }

    private IEnumerator WaitForParticleEnd()
    {
        yield return null;

        while (IsAnyParticleAlive())
        {
            yield return null;
        }

        pool.Despawn(gameObject);
    }

    private bool IsAnyParticleAlive()
    {
        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i].IsAlive(true))
            {
                return true;
            }
        }

        return false;
    }
}
