using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ArcherContinuousShootActiveSkill",
    menuName = "ScriptableObjects/SkillTree/Active/Archer Continuous Shoot"
)]
public sealed class ArcherContinuousShootActiveSkill : ActiveSkillData
{
    [SerializeField, Min(0f), Tooltip("화살 한 발당 데미지 배율")]
    private float damageRate = 1.0f;

    [SerializeField, Min(1), Tooltip("발사할 화살 개수")]
    private int arrowCount = 3;

    [SerializeField, Min(0f), Tooltip("화살 간 발사 간격 (초)")]
    private float shotInterval = 0.1f;

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
        if (target != null && target.Hp > 0f)
        {
            context.Executor.StartCoroutine(ShootRoutine(context, target));
        }
    }

    private IEnumerator ShootRoutine(ActiveSkillContext context, Entity target)
    {
        int shotsFired = 0;
        while (
            shotsFired < arrowCount && target != null && target.Hp > 0f && context.Caster != null
        )
        {
            float rawDamage = context.Caster.AttackPower * damageRate;
            rawDamage = context.GetModifiedDamage(rawDamage);

            (float damage, _) = DamageCalculator.Calculate(
                rawDamage,
                target.DamageReduction,
                context.Caster.CriticalPercentage,
                context.Caster.CriticalDamageMultiplier
            );

            float actualDamage = target.TakeDamage(damage, context.Caster);
            context.Caster.NotifyDamageDealt(target, actualDamage);

            shotsFired++;
            if (shotsFired < arrowCount)
            {
                yield return YieldInstructionCache.WaitForSeconds(shotInterval);
            }
        }
    }
}
