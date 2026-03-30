using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public int Hp { get; private set; }
    public float DamageReduction { get; private set; }
    public int AttackPower { get; private set; }
    public float AttackSpeed { get; private set; }
    public float CriticalPercentage { get; private set; }

    [SerializeField]
    private EntityStatData statData;

    protected virtual void Awake()
    {
        if (statData == null)
        {
            Debug.LogWarning("[경고] StatData가 비어 있습니다!");
            return;
        }

        Hp = statData.Hp;
        AttackPower = statData.AttackPower;
        CriticalPercentage = statData.CriticalPercentage;
    }

    public virtual void Attack() { }

    public virtual void Death() { }

    public virtual void TakeDamage(int damage)
    {
        Debug.Log($"데미지 받음: {damage}");
        Hp = Mathf.Max(0, Hp - damage);

        if (Hp <= 0)
            Death();
    }
}
