using UnityEngine;

/// <summary>
/// 감지 범위 안의 첫 번째 대상을 즉시 공격하는 공통 공격 방식입니다.
/// 투사체를 생성하지 않고 피해 판정을 먼저 처리한 뒤 엔티티가 공격 애니메이션을 실행합니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class SingleTargetAttackBehaviour : EntityAttackBehaviour
{
    [SerializeField]
    private AttackTargetsSensing targets;

    private void Awake()
    {
        if (targets == null)
            targets = GetComponentInChildren<AttackTargetsSensing>();
    }

    public override bool TryAttack(Entity attacker)
    {
        if (attacker == null || targets == null)
            return false;

        Entity target = targets.GetFirstTarget();
        if (target == null)
            return false;

        (float damage, _) = DamageCalculator.Calculate(
            attacker.AttackPower,
            target.DamageReduction,
            attacker.CriticalPercentage,
            attacker.CriticalDamageMultiplier
        );

        SkillTreeComponent skillTreeComponent = null;
        if (attacker.TryGetComponent(out SkillTreeComponent foundSkillTreeComponent))
        {
            skillTreeComponent = foundSkillTreeComponent;
            damage *= skillTreeComponent.GetOutgoingDamageMultiplier();
            damage *= skillTreeComponent.GetBasicAttackDamageMultiplier();
        }

        if (target is Enemy { IsEliteTarget: true } && skillTreeComponent != null)
        {
            damage *= skillTreeComponent.GetEliteDamageMultiplier();
        }

        float actualDamage = target.TakeDamage(damage, attacker);
        attacker.NotifyDamageDealt(target, actualDamage);
        return true;
    }

    private void OnValidate()
    {
        if (targets == null)
            targets = GetComponentInChildren<AttackTargetsSensing>();

        if (targets == null)
        {
            Debug.LogError(
                "[SingleTargetAttackBehaviour] AttackTargetsSensing을 할당해주세요.",
                this
            );
        }
    }
}
