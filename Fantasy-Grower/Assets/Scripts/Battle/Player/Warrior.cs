using UnityEngine;

public class Warrior : Player
{
    public override void Attack()
    {
        // TODO : 플레이어 애니메이션 효과 적용

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
