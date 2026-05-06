using UnityEngine;

public class Player : Entity
{
    [SerializeField, Header("공격 설정")]
    protected AttackTargetsSensing targets;

    public override void Attack()
    {
        entityState[gameObject].State = PlayerState.ATTACK; // 공격 상태로 전환하여 애니메이션과 공격 로직이 실행되도록 함
    }

    public override void Death()
    {
        base.Death();

        entityState[gameObject].State = PlayerState.DEATH; // 사망 상태로 전환하여 애니메이션과 사망 로직이 실행되도록 함
    }

    protected override void OnValidate()
    {
        if (!targets)
            Debug.LogError(
                "[Entity] Targets 필드에 AttackTargetsSensing 컴포넌트를 할당해주세요.",
                this
            );

        base.OnValidate();
    }
}
