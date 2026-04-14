using System;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public int Hp { get; private set; }
    public int MaxHp { get; private set; }
    public float DamageReduction { get; private set; }
    public int AttackPower { get; private set; }
    public float AttackSpeed { get; private set; }
    public float CriticalPercentage { get; private set; }
    public EntityType EntityType { get; private set; }

    [SerializeField]
    private EntityStatData statData;

    /// <summary>HP가 0이 되어 Death()가 호출될 때 발화된다.</summary>
    public event Action<Entity> OnDied;

    protected virtual void Awake()
    {
        if (statData == null)
        {
            Debug.LogWarning("[경고] StatData가 비어 있습니다!");
            return;
        }

        Hp = statData.Hp;
        MaxHp = statData.Hp;
        DamageReduction = statData.DamageReduction;
        AttackPower = statData.AttackPower;
        AttackSpeed = statData.AttackSpeed;
        CriticalPercentage = statData.CriticalPercentage;
    }

    public virtual void Attack() { }

    /// <summary>HP를 MaxHp로 복구한다. 던전 재시도 시 사용.</summary>
    public void ResetHp()
    {
        Hp = MaxHp;
    }

    public virtual void Death()
    {
        OnDied?.Invoke(this);
    }

    public virtual void TakeDamage(int damage)
    {
        if (Hp <= 0)
            return; // 이미 사망 — 중복 호출 무시

        Debug.Log($"데미지 받음: {damage}");
        Hp = Mathf.Max(0, Hp - damage);

        if (Hp <= 0)
            Death();
    }

    /// <summary>
    /// 해금된 패시브 스킬 효과를 스탯에 반영한다.
    /// SkillTreeComponent.RecalculatePassives()에서 패시브 집계 후 호출된다.
    /// statData 기반값에 modifier를 합산하여 런타임 스탯을 갱신한다.
    /// </summary>
    public void ApplyStatModifier(EntityStatModifier modifier)
    {
        if (statData == null)
            return;

        // 현재 체력 비율에 맞춰 회복
        float hpRatio = MaxHp > 0 ? (float)Hp / MaxHp : 1f;
        MaxHp = statData.Hp + modifier.BonusHp;
        Hp = Mathf.RoundToInt(MaxHp * hpRatio);

        DamageReduction = statData.DamageReduction + modifier.BonusDamageReduction;
        AttackPower = statData.AttackPower + modifier.BonusAttackPower;
        AttackSpeed = statData.AttackSpeed + modifier.BonusAttackSpeed;
        CriticalPercentage = statData.CriticalPercentage + modifier.BonusCriticalPercentage;
    }
}
