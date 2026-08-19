using UnityEngine;

public class Player : Entity
{
    public override void Death()
    {
        base.Death();

        entityState[gameObject].State = PlayerState.DEATH; // 사망 상태로 전환하여 애니메이션과 사망 로직이 실행되도록 함
    }
}
