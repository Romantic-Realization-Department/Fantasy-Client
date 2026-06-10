using UnityEngine;

/// <summary>
/// 데미지 계산 공식을 담당하는 정적 유틸리티.
/// AttackCollider와 EnemyAI에서 공통으로 사용한다.
/// </summary>
public static class DamageCalculator
{
    /// <summary>
    /// 최종 데미지와 크리티컬 여부를 계산한다.
    /// </summary>
    /// <param name="rawAttackPower">공격자의 공격력</param>
    /// <param name="targetDamageReduction">피격자의 피해 감소율 (0.0 ~ 1.0, 예: 0.2 = 20% 감소)</param>
    /// <param name="attackerCriticalPercentage">공격자의 크리티컬 확률 (0 ~ 100, 예: 25f = 25%)</param>
    /// <returns>(최종 데미지, 크리티컬 여부)</returns>
    public static (float damage, bool isCritical) Calculate(
        float rawAttackPower,
        float targetDamageReduction,
        float attackerCriticalPercentage
    )
    {
        float reduced = Mathf.Max(1, rawAttackPower * (1f - targetDamageReduction));
        bool isCritical = Random.value * 100f < attackerCriticalPercentage;
        float finalDamage = isCritical ? reduced * 2 : reduced;
        return (finalDamage, isCritical);
    }
}
