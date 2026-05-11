using System.Collections.Generic;
using UnityEngine;

public class HitEffectObjPool : MonoBehaviour
{
    private static HitEffectObjPool instance;

    [SerializeField] private GameObject spawnObject;
    [SerializeField] private int poolSize = 10;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        instance = this;

        CreatePool(poolSize);
    }

    private void CreatePool(int size)
    {
        for (int i = 0; i < size; i++)
        {
            GameObject obj = Instantiate(spawnObject, transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public static GameObject Spawn(Vector3 position, Quaternion rotation)
    {
        if (instance.pool.Count <= 0)
        {
            instance.CreatePool(1);
        }

        GameObject obj = instance.pool.Dequeue();

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        if (obj.TryGetComponent<HitEffectAutoDespawn>(out HitEffectAutoDespawn autoDespawn))
        {
            autoDespawn.Play(instance);
        }

        return obj;
    }

    public void Despawn(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
