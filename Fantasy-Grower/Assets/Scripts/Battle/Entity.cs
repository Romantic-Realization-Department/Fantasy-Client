using System;
using UnityEngine;

public enum EntityType
{
    Player,
    Enemy,
}

public abstract class Entity : MonoBehaviour
{
    public float Hp { get; private set; }
    public float MaxHp { get; private set; }
    public float HpRecovery { get; private set; }
    public float DamageReduction { get; private set; }
    public float AttackPower { get; private set; }
    public float AttackSpeed { get; private set; }
    public float CriticalPercentage { get; private set; }
    public float CriticalDamageMultiplier { get; private set; } = 2f;
    public float AttackRange { get; private set; } = 1f;
    public float MoveSpeedMultiplier { get; private set; } = 1f;

    [field: SerializeField, Header("엔티티 설정")]
    public EntityType EntityType { get; private set; }

    [SerializeField]
    protected EntityStatData statData;

    protected EntityState entityState = EntityState.Instance;

    // 패시브 슬롯, 장비, 액티브 버프 인스턴스마다 고유 Handle을 발급해 독립적으로 중첩하고 제거합니다.
    private readonly System.Collections.Generic.Dictionary<int, EntityStatModifier> statModifiers =
        new();
    private int nextStatModifierId = 1;

    /// <summary>HP가 0이 되어 Death()가 호출될 때 발화된다.</summary>
    public event Action<Entity> OnDied;

    /// <summary>실제 피해로 HP가 감소했을 때 피해 직전과 직후의 HP를 순서대로 전달한다.</summary>
    public event Action<float, float> OnDamageTaken;

    /// <summary>피해가 HP에 적용되기 전에 발화된다. 패시브가 피해량을 조정하거나 취소할 수 있다.</summary>
    public event Action<IncomingDamageContext> OnBeforeDamageTaken;

    /// <summary>다른 Entity에게 피해를 입혔을 때 대상과 실제 피해량을 전달한다.</summary>
    public event Action<Entity, float> OnDamageDealt;

    /// <summary>Update문이 호출될 때 발화된다.</summary>
    public event Action OnUpdated;

    /// <summary>스탯 재계산이 끝났을 때 발화된다.</summary>
    public event Action OnStatsChanged;

    protected virtual void Awake()
    {
        if (statData == null)
        {
            Debug.LogWarning("[경고] StatData가 비어 있습니다!");
            return;
        }

        RecalculateStats();
        entityState[gameObject].OnStateChanged += OnStateChanged;
    }

    protected virtual void Start()
    {
        entityState[gameObject].State = PlayerState.IDLE;
    }

    protected void Update()
    {
        // 불변 영역(엔티티 기본 체력 회복)
        if (Hp > 0)
        {
            Hp = Mathf.MoveTowards(Hp, MaxHp, HpRecovery * Time.deltaTime);
        }

        // 가변 영역
        OnUpdated?.Invoke();
    }

    private void OnStateChanged(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.IDLE:
                OnIdle();
                break;
            case PlayerState.MOVE:
                OnMove();
                break;
            case PlayerState.ATTACK:
                OnAttack();
                break;
            case PlayerState.DAMAGED:
                OnDamaged();
                break;
            case PlayerState.DEBUFF:
                OnDebuff();
                break;
            case PlayerState.DEATH:
                OnDeath();
                break;
            case PlayerState.OTHER:
                OnOther();
                break;
        }
    }

    #region 상태 변경 로직

    protected virtual void OnIdle() { }

    protected virtual void OnMove() { }

    protected virtual void OnAttack() { }

    protected virtual void OnDamaged() { }

    protected virtual void OnDebuff() { }

    protected virtual void OnDeath() { }

    protected virtual void OnOther() { }

    #endregion

    // 실제 공격 판정은 공격 가능한 자식 클래스가 처리하고, Entity는 공격 상태만 전환합니다.
    public virtual void Attack()
    {
        entityState[gameObject].State = PlayerState.ATTACK;
    }

    /// <summary>HP를 MaxHp로 복구한다. 던전 재시도 시 사용.</summary>
    public void ResetHp()
    {
        Hp = MaxHp;
    }

    public virtual void Death()
    {
        OnDied?.Invoke(this);
        entityState[gameObject].State = PlayerState.DEATH;
    }

    public virtual float TakeDamage(float damage, Entity attacker = null)
    {
        if (Hp <= 0)
            return 0f; // 이미 사망 — 중복 호출 무시

        var damageContext = new IncomingDamageContext(this, attacker, damage);
        OnBeforeDamageTaken?.Invoke(damageContext);

        if (damageContext.IsCancelled || damageContext.Damage <= 0f)
            return 0f;

        damage = damageContext.Damage;
        Debug.Log($"데미지 받음: {damage}");
        float previousHp = Hp;
        Hp = Mathf.Max(0, Hp - damage);
        float actualDamage = previousHp - Hp;

        if (Hp < previousHp)
            OnDamageTaken?.Invoke(previousHp, Hp);

        entityState[gameObject].State = PlayerState.DAMAGED;

        if (Hp <= 0)
            Death();

        return actualDamage;
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || Hp <= 0f)
            return;

        Hp = Mathf.Min(MaxHp, Hp + amount);
    }

    public void NotifyDamageDealt(Entity target, float damage)
    {
        if (target == null || damage <= 0f)
            return;

        OnDamageDealt?.Invoke(target, damage);
    }

    /// <summary>
    /// 새로운 스탯 보정값을 추가하고 해당 효과만 갱신하거나 제거할 수 있는 고유 Handle을 반환합니다.
    /// 같은 스킬이 여러 번 사용되어도 호출마다 다른 Handle이 발급되므로 각각 독립적으로 중첩됩니다.
    /// </summary>
    public EntityStatModifierHandle ApplyStatModifier(EntityStatModifier modifier)
    {
        int modifierId = nextStatModifierId++;
        if (nextStatModifierId <= 0)
            nextStatModifierId = 1;

        while (statModifiers.ContainsKey(modifierId))
        {
            modifierId = nextStatModifierId++;
            if (nextStatModifierId <= 0)
                nextStatModifierId = 1;
        }

        statModifiers[modifierId] = modifier;
        RecalculateStats();
        return new EntityStatModifierHandle(modifierId);
    }

    /// <summary>
    /// 기존 Handle의 보정값을 교체합니다. 장비 강화처럼 같은 효과의 수치만 바뀔 때 사용합니다.
    /// </summary>
    public bool UpdateStatModifier(EntityStatModifierHandle handle, EntityStatModifier modifier)
    {
        if (!handle.IsValid || !statModifiers.ContainsKey(handle.Id))
            return false;

        statModifiers[handle.Id] = modifier;
        RecalculateStats();
        return true;
    }

    /// <summary>
    /// Handle에 대응하는 스탯 보정값 하나만 제거합니다.
    /// </summary>
    public bool RemoveStatModifier(EntityStatModifierHandle handle)
    {
        if (!handle.IsValid || !statModifiers.Remove(handle.Id))
            return false;

        RecalculateStats();
        return true;
    }

    /// <summary>
    /// 모든 출처의 스탯 보정값을 제거하고 기본 스탯으로 되돌립니다.
    /// </summary>
    public void ClearStatModifiers()
    {
        if (statModifiers.Count == 0)
            return;

        statModifiers.Clear();
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        if (statData == null)
            return;

        EntityStatModifier totalModifier = EntityStatModifier.Zero;
        foreach (EntityStatModifier modifier in statModifiers.Values)
            totalModifier += modifier;

        // 최대 체력이 바뀌어도 현재 체력의 비율은 유지합니다.
        float hpRatio = MaxHp > 0f ? Mathf.Clamp01(Hp / MaxHp) : 1f;

        MaxHp = CalculateModifiedStat(
            statData.Hp,
            totalModifier.BonusHp,
            totalModifier.BonusHpRate,
            1f
        );
        Hp = Mathf.Clamp(MaxHp * hpRatio, 0f, MaxHp);

        HpRecovery = CalculateModifiedStat(
            statData.HpRecovery,
            totalModifier.BonusHpRecovery,
            totalModifier.BonusHpRecoveryRate
        );
        DamageReduction = Mathf.Clamp01(
            CalculateModifiedStat(
                statData.DamageReduction,
                totalModifier.BonusDamageReduction,
                totalModifier.BonusDamageReductionRate
            )
        );
        AttackPower = CalculateModifiedStat(
            statData.AttackPower,
            totalModifier.BonusAttackPower,
            totalModifier.BonusAttackPowerRate
        );
        AttackSpeed = CalculateModifiedStat(
            statData.AttackSpeed,
            totalModifier.BonusAttackSpeed,
            totalModifier.BonusAttackSpeedRate
        );
        CriticalPercentage = Mathf.Clamp(
            CalculateModifiedStat(
                statData.CriticalPercentage,
                totalModifier.BonusCriticalPercentage,
                totalModifier.BonusCriticalPercentageRate
            ),
            0f,
            100f
        );
        CriticalDamageMultiplier = Mathf.Max(1f, 2f * (1f + totalModifier.BonusCriticalDamageRate));
        AttackRange = CalculateModifiedStat(
            statData.AttackRange,
            0f,
            totalModifier.BonusAttackRangeRate
        );
        MoveSpeedMultiplier = Mathf.Max(0f, 1f + totalModifier.BonusMoveSpeedRate);
        OnStatsChanged?.Invoke();
    }

    private static float CalculateModifiedStat(
        float baseValue,
        float flatBonus,
        float bonusRate,
        float minimumValue = 0f
    )
    {
        // 모든 고정값을 먼저 더한 뒤 비율 보정끼리 합산해 곱합니다.
        // 예: 기본 100 + 고정 20, 비율 0.1 + 0.2 => 120 * 1.3 = 156
        return Mathf.Max(minimumValue, (baseValue + flatBonus) * (1f + bonusRate));
    }

    protected virtual void OnDestroy()
    {
        entityState[gameObject].OnStateChanged -= OnStateChanged;
    }

    protected virtual void OnValidate()
    {
        if (!statData)
            Debug.LogWarning("[Entity] StatData가 비어 있습니다!", this);
    }
}
