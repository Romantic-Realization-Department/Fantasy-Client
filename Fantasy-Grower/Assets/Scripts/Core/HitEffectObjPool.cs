using System.Collections.Generic;
using UnityEngine;

public class HitEffectObjPool : MonoBehaviour
{
    private static HitEffectObjPool instance;

    [SerializeField]
    private GameObject spawnObject;

    [SerializeField]
    private int poolSize = 10;

    private readonly Queue<HitEffectAutoDespawn> pool = new Queue<HitEffectAutoDespawn>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        CreatePool(poolSize);
    }

    private void CreatePool(int size)
    {
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(spawnObject, transform);

            if (!obj.TryGetComponent(out HitEffectAutoDespawn effect))
            {
                Debug.LogError($"{spawnObject.name} 프리팹에 HitEffectAutoDespawn이 없습니다.");
                Destroy(obj);
                continue;
            }

            obj.SetActive(false);
            pool.Enqueue(effect);
        }
    }

    public static GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        if (instance == null)
            return null;

        if (instance.pool.Count <= 0)
        {
            instance.CreatePool(1);
        }

        HitEffectAutoDespawn effect = instance.pool.Dequeue();

        effect.transform.SetPositionAndRotation(position, rotation);
        effect.gameObject.SetActive(true);
        effect.Play(instance);

        return effect.gameObject;
    }

    public void Despawn(HitEffectAutoDespawn effect)
    {
        if (effect == null)
            return;

        effect.gameObject.SetActive(false);
        pool.Enqueue(effect);
    }
}
