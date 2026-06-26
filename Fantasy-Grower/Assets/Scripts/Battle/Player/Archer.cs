using UnityEngine;

public class Archer : Player
{
    public override void Attack()
    {
        // 플레이어 애니메이션 효과 적용
        base.Attack();

        // 공격 범위 내의 적을 감지하여 데미지를 입히는 로직
        Entity target = targets.GetFirstTarget();
        if (target == null)
            return;

        var (damage, _) = DamageCalculator.Calculate(
            AttackPower,
            target.DamageReduction,
            CriticalPercentage
        );
        target.TakeDamage(damage);
    }
}
