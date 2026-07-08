using UnityEngine;

[CreateAssetMenu(
    fileName = "TargetedDamageActiveSkill",
    menuName = "ScriptableObjects/SkillTree/Active/Targeted Damage"
)]
public sealed class TargetedDamageActiveSkill : ActiveSkillData
{
    [SerializeField, Min(0f)]
    private float baseDamage;

    [SerializeField, Min(0f)]
    private float attackPowerRate;

    [SerializeField, Min(1)]
    private int maxTargets = 1;

    [SerializeField]
    private ActiveSkillTargetMode targetMode = ActiveSkillTargetMode.DetectedTargets;

    [SerializeField]
    private bool guaranteedCritical;

    [SerializeField, Range(0f, 1f)]
    private float lifeStealRate;

    [SerializeField]
    private bool applyStatusEffect;

    [SerializeField]
    private StatusEffectType statusEffectType;

    [SerializeField, Min(0f)]
    private float statusEffectDuration = 5f;

    [SerializeField]
    private EntityStatModifier statusModifier;

    [SerializeField, Min(0f)]
    private float incomingDamageBonusRate;

    protected override bool CanUseSkill(ActiveSkillContext context)
    {
        return base.CanUseSkill(context)
            && context.CollectTargets(targetMode, maxTargets, ExtensionRange).Count > 0;
    }

    protected override void UseSkill(ActiveSkillContext context)
    {
        var targets = context.CollectTargets(targetMode, maxTargets, ExtensionRange);
        float rawDamage = baseDamage + context.Caster.AttackPower * attackPowerRate;
        rawDamage = context.GetModifiedDamage(rawDamage);

        float totalActualDamage = 0f;
        for (int i = 0; i < targets.Count; i++)
        {
            Entity target = targets[i];
            if (target == null || target.Hp <= 0f)
                continue;

            float criticalChance = guaranteedCritical ? 100f : context.Caster.CriticalPercentage;
            (float damage, _) = DamageCalculator.Calculate(
                rawDamage,
                target.DamageReduction,
                criticalChance,
                context.Caster.CriticalDamageMultiplier
            );

            float actualDamage = target.TakeDamage(damage, context.Caster);
            totalActualDamage += actualDamage;
            context.Caster.NotifyDamageDealt(target, actualDamage);

            ApplyStatusEffect(context.Caster, target);
        }

        if (lifeStealRate > 0f && totalActualDamage > 0f)
            context.Caster.Heal(totalActualDamage * lifeStealRate);
    }

    private void ApplyStatusEffect(Entity source, Entity target)
    {
        if (!applyStatusEffect || target == null || statusEffectDuration <= 0f)
            return;

        StatusEffectController controller = StatusEffectController.GetOrAdd(target);
        if (controller == null)
            return;

        if (statusEffectType == StatusEffectType.Stun)
        {
            controller.ApplyActionBlock(StatusEffectType.Stun, source, statusEffectDuration);
            return;
        }

        if (statusEffectType == StatusEffectType.IncomingDamageUp)
        {
            controller.ApplyIncomingDamageUp(
                source,
                incomingDamageBonusRate,
                statusEffectDuration,
                false,
                1
            );
            return;
        }

        controller.ApplyModifierEffect(
            statusEffectType,
            source,
            statusModifier,
            statusEffectDuration,
            false,
            1
        );
    }
}
