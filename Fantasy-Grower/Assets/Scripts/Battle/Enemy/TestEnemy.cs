using System.Collections;
using UnityEngine;

public class TestEnemy : Enemy
{
    public override void Death()
    {
        Debug.Log("TestEnemy 죽음");
        base.Death(); // Enemy.Death() → 보상 지급 + OnDied 이벤트
    }

    public override void Attack()
    {
        // TODO : 적 공격 애니메이션 효과 적용

        // 공격 범위 내의 적을 감지하여 데미지를 입히는 로직
        foreach (Entity target in Targets.GetTargets())
        {
            bool shouldHit = (EntityType != target.EntityType);
            if (!shouldHit)
                continue;
            var (damage, _) = DamageCalculator.Calculate(
                AttackPower,
                target.DamageReduction,
                CriticalPercentage
            );
            target.TakeDamage(damage);
        }
    }
}
