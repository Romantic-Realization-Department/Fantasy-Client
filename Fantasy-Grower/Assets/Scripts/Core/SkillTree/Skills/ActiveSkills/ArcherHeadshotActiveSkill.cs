using UnityEngine;

[CreateAssetMenu(
    fileName = "ArcherHeadshotActiveSkill",
    menuName = "ScriptableObjects/SkillTree/Active/Archer Headshot"
)]
public sealed class ArcherHeadshotActiveSkill : ActiveSkillData
{
    [SerializeField, Range(0f, 1f), Tooltip("타겟의 최대 체력 대비 피해량 비율 (예: 0.1 = 10%)")]
    private float targetMaxHpDamageRate = 0.1f;

    protected override bool CanUseSkill(ActiveSkillContext context)
    {
        return base.CanUseSkill(context)
            && context
                .CollectTargets(ActiveSkillTargetMode.DetectedTargets, 1, ExtensionRange)
                .Count > 0;
    }

    protected override void UseSkill(ActiveSkillContext context)
    {
        var targets = context.CollectTargets(
            ActiveSkillTargetMode.DetectedTargets,
            1,
            ExtensionRange
        );
        if (targets.Count == 0)
            return;

        Entity target = targets[0];
        if (target == null || target.Hp <= 0f)
            return;

        // 최대 체력 비례 데미지 계산
        float rawDamage = target.MaxHp * targetMaxHpDamageRate;
        rawDamage = context.GetModifiedDamage(rawDamage);

        (float damage, _) = DamageCalculator.Calculate(
            rawDamage,
            target.DamageReduction,
            context.Caster.CriticalPercentage,
            context.Caster.CriticalDamageMultiplier
        );

        float actualDamage = target.TakeDamage(damage, context.Caster);
        context.Caster.NotifyDamageDealt(target, actualDamage);
    }
}
