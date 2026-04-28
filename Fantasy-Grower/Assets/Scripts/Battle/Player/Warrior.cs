public class Warrior : Player
{
    public override void Attack()
    {
        // TODO : 플레이어 애니메이션 효과 적용

        // 공격 범위 내의 적을 감지하여 데미지를 입히는 로직
        Entity target = targets.GetFirstTarget();

        var (damage, _) = DamageCalculator.Calculate(
            AttackPower,
            target.DamageReduction,
            CriticalPercentage
        );
        target.TakeDamage(damage);
    }
}
