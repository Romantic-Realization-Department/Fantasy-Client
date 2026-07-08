using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ArrowRainActiveSkill",
    menuName = "ScriptableObjects/SkillTree/Active/Arrow Rain"
)]
public sealed class ArrowRainActiveSkill : ActiveSkillData
{
    [SerializeField, Min(0f), Tooltip("지속 시간 동안 입힐 총 기본 피해량")]
    private float totalBaseDamage = 50f;

    [SerializeField, Min(0f), Tooltip("지속 시간 동안 입힐 총 공격력 계수")]
    private float totalAttackPowerRate = 2.0f;

    [SerializeField, Min(0.1f), Tooltip("장판 지속 시간")]
    private float duration = 3.0f;

    [SerializeField, Min(0.1f), Tooltip("피해 틱 주기 (초)")]
    private float tickInterval = 0.5f;

    [SerializeField, Range(0f, 1f), Tooltip("이동 속도 감소 비율 (0.5 = 50% 감소)")]
    private float slowRate = 0.5f;

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

        Vector3 targetPosition = targets[0].transform.position;
        context.Executor.StartCoroutine(HazardRoutine(context, targetPosition));
    }

    private IEnumerator HazardRoutine(ActiveSkillContext context, Vector3 centerPosition)
    {
        int tickCount = Mathf.Max(1, Mathf.RoundToInt(duration / tickInterval));
        float baseDamagePerTick = totalBaseDamage / tickCount;
        float attackPowerRatePerTick = totalAttackPowerRate / tickCount;

        float elapsed = 0f;
        List<Entity> targetsBuffer = new List<Entity>();
        List<Entity> allEnemies = new List<Entity>();

        // 이동 속도 감소 수치 정의
        EntityStatModifier slowModifier = EntityStatModifier.Zero;
        slowModifier.BonusMoveSpeedRate = -slowRate;

        while (elapsed < duration && context.Caster != null)
        {
            // 전체 적 중 장판 범위 내에 있는지 판정
            WaveController.TryCollectActiveEnemies(allEnemies);

            float attackAreaMultiplier =
                context.SkillTreeComponent != null
                    ? context.SkillTreeComponent.GetAttackAreaMultiplier()
                    : 1f;
            float searchRange = ExtensionRange * attackAreaMultiplier;
            float searchRangeSqr = searchRange * searchRange;

            targetsBuffer.Clear();
            for (int i = 0; i < allEnemies.Count; i++)
            {
                Entity enemy = allEnemies[i];
                if (enemy == null || enemy.Hp <= 0f)
                    continue;

                float sqrDist = (enemy.transform.position - centerPosition).sqrMagnitude;
                if (sqrDist <= searchRangeSqr)
                {
                    targetsBuffer.Add(enemy);
                }
            }

            float rawDamage =
                baseDamagePerTick + context.Caster.AttackPower * attackPowerRatePerTick;
            rawDamage = context.GetModifiedDamage(rawDamage);

            for (int i = 0; i < targetsBuffer.Count; i++)
            {
                Entity target = targetsBuffer[i];
                if (target == null || target.Hp <= 0f)
                    continue;

                // 슬로우 디버프 적용 (다음 틱 전에 풀리지 않도록 1.5배의 지속시간 부여)
                StatusEffectController controller = StatusEffectController.GetOrAdd(target);
                if (controller != null)
                {
                    controller.ApplyModifierEffect(
                        StatusEffectType.MoveSpeedDown,
                        context.Caster,
                        slowModifier,
                        tickInterval * 1.5f,
                        false,
                        1
                    );
                }

                // 피해 판정
                (float damage, _) = DamageCalculator.Calculate(
                    rawDamage,
                    target.DamageReduction,
                    context.Caster.CriticalPercentage,
                    context.Caster.CriticalDamageMultiplier
                );

                float actualDamage = target.TakeDamage(damage, context.Caster);
                context.Caster.NotifyDamageDealt(target, actualDamage);
            }

            yield return YieldInstructionCache.WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }
    }
}
