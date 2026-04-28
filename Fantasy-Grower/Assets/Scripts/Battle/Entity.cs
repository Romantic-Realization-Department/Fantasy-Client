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
    public float CriticalPercentage { get; private set; }

    [field: SerializeField, Header("엔티티 설정")]
    public EntityType EntityType { get; private set; }

    [SerializeField]
    protected EntityStatData statData;

    protected EntityState entityState = EntityState.Instance;

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
        entityState[gameObject].OnStateChanged += OnStateChanged;
    }

    protected virtual void Start()
    {
        entityState[gameObject].State = PlayerState.IDLE;
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

    // 애니메이션의 특정 시점에서 호출하고 싶다면, StateMachineBehaviour를 거쳐 호출하는 것을 추천합니다.
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
