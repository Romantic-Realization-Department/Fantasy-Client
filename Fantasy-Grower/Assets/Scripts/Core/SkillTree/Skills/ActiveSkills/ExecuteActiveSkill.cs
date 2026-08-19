using UnityEngine;

[CreateAssetMenu(
    fileName = "ExecuteActiveSkill",
    menuName = "ScriptableObjects/SkillTree/Active/Warrior Execute"
)]
public sealed class ExecuteActiveSkill : ActiveSkillData
{
    [SerializeField, Min(0f)]
    private float baseDamage = 200f;

    [SerializeField, Min(0f)]
    private float missingHpDamageRate = 0.2f;

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

        float missingHp = Mathf.Max(0f, target.MaxHp - target.Hp);
        float rawDamage = context.GetModifiedDamage(baseDamage + missingHp * missingHpDamageRate);
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
