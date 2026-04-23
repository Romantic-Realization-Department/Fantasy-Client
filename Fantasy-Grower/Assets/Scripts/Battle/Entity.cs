using System;
using UnityEngine;

public enum EntityType
{
    Player,
    Enemy,
}

public abstract class Entity : MonoBehaviour
{
    public int Hp { get; private set; }
    public int MaxHp { get; private set; }
    public float DamageReduction { get; private set; }
    public int AttackPower { get; private set; }
    public float AttackSpeed { get; private set; }
    public float AttackRange { get; private set; }
    public float CriticalPercentage { get; private set; }

    [field: SerializeField]
    public EntityType EntityType { get; private set; }

    [SerializeField]
    protected EntityStatData statData;

    /// <summary>HP가 0이 되어 Death()가 호출될 때 발화된다.</summary>
    public event Action<Entity> OnDied;

    [SerializeField, Tooltip("플레이어 일 때는 1, 적 일 떄는 -1이 기본값")]
    protected int entityDirection;

    [SerializeField, Tooltip("타겟 레이어")]
    protected LayerMask targetLayer;

    protected abstract int MaxEntityCount { get; }

    private Collider2D[] entities;
    private ContactFilter2D contactFilter;

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
        AttackRange = statData.AttackRange;
        CriticalPercentage = statData.CriticalPercentage;
        entityDirection =
            entityDirection == 0 ? (EntityType == EntityType.Player ? 1 : -1) : entityDirection;
        contactFilter.useLayerMask = true;
        contactFilter.SetLayerMask(targetLayer);
        entities = new Collider2D[MaxEntityCount];
    }

    public virtual void Attack()
    {
        // 애니메이션의 특정 시점에서 호출하고 싶다면, StateMachineBehaviour를 활용하여 호출하는 것을 추천합니다.

        // 공격 범위 내의 적을 감지하여 데미지를 입히는 로직
        int hitCount = Physics2D.OverlapBox(
            transform.position + new Vector3(AttackRange * entityDirection / 2f, 0),
            new Vector2(AttackRange, 1),
            0f,
            contactFilter,
            entities
        );

        for (int i = 0; i < hitCount; i++)
        {
            if (entities[i].TryGetComponent(out Entity target))
            {
                bool shouldHit = (EntityType != target.EntityType);
                if (!shouldHit)
                    continue;
                var (damage, _) = DamageCalculator.Calculate(
                    AttackPower,
                    target.DamageReduction,
                    CriticalPercentage
                );
                target.TakeDamage(damage);
            }
        }
    }

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

    private void OnDrawGizmosSelected()
    {
        if (statData)
        {
            Gizmos.color = Color.red;
            Vector3 attackBoxCenter =
                transform.position + new Vector3(statData.AttackRange * entityDirection / 2f, 0);
            Vector3 attackBoxSize = new Vector3(statData.AttackRange, 1, 0);
            Gizmos.DrawWireCube(attackBoxCenter, attackBoxSize);
        }
    }
}
