using System.Collections.Generic;
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

    private readonly List<Entity> extensionTargetBuffer = new();
    private readonly List<Entity> nearestTargetBuffer = new();

    private void Awake()
    {
        if (targets == null)
            targets = GetComponentInChildren<AttackTargetsSensing>();
    }

    public override bool TryAttack(Entity attacker)
    {
        if (attacker == null || targets == null)
            return false;

        SkillTreeComponent skillTreeComponent = null;
        BasicAttackSkillData basicAttack = null;
        if (attacker.TryGetComponent(out SkillTreeComponent foundSkillTreeComponent))
        {
            skillTreeComponent = foundSkillTreeComponent;
            basicAttack = skillTreeComponent.GetUnlockedBasicAttack();
        }

        // 인식 사거리 콜라이더에 적이 1마리라도 있어야 공격 모션을 시작함
        var sensingTargets = targets.GetTargets();
        if (sensingTargets == null || sensingTargets.Count == 0)
            return false;

        int maxTargets = basicAttack != null ? basicAttack.MaxTargets : 1;

        // 연장 사거리가 있으면 거리 기반 O(N*K) 탐색으로 타겟을 수집
        System.Collections.Generic.IReadOnlyList<Entity> attackTargets;
        if (basicAttack != null && basicAttack.ExtensionRange > 0f && maxTargets > 1)
        {
            float totalRange =
                (attacker.AttackRange > 0f ? attacker.AttackRange : 0f)
                + basicAttack.ExtensionRange;
            WaveController.TryCollectActiveEnemies(extensionTargetBuffer);
            for (int j = extensionTargetBuffer.Count - 1; j >= 0; j--)
            {
                Entity e = extensionTargetBuffer[j];
                if (e == null || e.Hp <= 0f)
                    extensionTargetBuffer.RemoveAt(j);
            }
            CollectNearestInRange(
                extensionTargetBuffer,
                attacker.transform.position,
                totalRange,
                maxTargets,
                nearestTargetBuffer
            );
            attackTargets = nearestTargetBuffer;
        }
        else
        {
            attackTargets = sensingTargets;
        }

        int attackedCount = 0;
        for (int i = 0; i < attackTargets.Count && attackedCount < maxTargets; i++)
        {
            Entity target = attackTargets[i];
            if (target == null || target.Hp <= 0f)
                continue;

            (float damage, _) = DamageCalculator.Calculate(
                attacker.AttackPower,
                target.DamageReduction,
                attacker.CriticalPercentage,
                attacker.CriticalDamageMultiplier
            );

            if (basicAttack != null)
                damage *= basicAttack.DamageRate;

            if (skillTreeComponent != null)
            {
                damage *= skillTreeComponent.GetOutgoingDamageMultiplier();
                damage *= skillTreeComponent.GetBasicAttackDamageMultiplier();
            }
            damage *= attacker.OutgoingDamageMultiplier;

            if (target is Enemy { IsEliteTarget: true } && skillTreeComponent != null)
            {
                damage *= skillTreeComponent.GetEliteDamageMultiplier();
            }

            float actualDamage = target.TakeDamage(damage, attacker);
            attacker.NotifyDamageDealt(target, actualDamage);
            attackedCount++;
        }

        return attackedCount > 0;
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

    /// <summary>
    /// 원점으로부터 totalRange 이내의 생존 적 중 가장 가까운 K마리를 수집합니다.
    /// O(N * K) 삽입 방식: 람다·Sort·Sqrt 없이 sqrMagnitude만 비교합니다.
    /// </summary>
    private static void CollectNearestInRange(
        List<Entity> candidates,
        Vector3 origin,
        float totalRange,
        int maxTargets,
        List<Entity> results
    )
    {
        results.Clear();
        float rangeSqr = totalRange * totalRange;
        int k = maxTargets > 0 ? maxTargets : int.MaxValue;

        for (int i = 0; i < candidates.Count; i++)
        {
            Entity candidate = candidates[i];
            if (candidate == null || candidate.Hp <= 0f)
                continue;

            float sqrDist = (candidate.transform.position - origin).sqrMagnitude;
            if (sqrDist > rangeSqr)
                continue;

            int insertIndex = results.Count;
            for (int j = results.Count - 1; j >= 0; j--)
            {
                float existingSqrDist = (results[j].transform.position - origin).sqrMagnitude;
                if (sqrDist < existingSqrDist)
                    insertIndex = j;
                else
                    break;
            }

            if (insertIndex < k)
            {
                results.Insert(insertIndex, candidate);

                if (results.Count > k)
                    results.RemoveAt(results.Count - 1);
            }
        }
    }
}
