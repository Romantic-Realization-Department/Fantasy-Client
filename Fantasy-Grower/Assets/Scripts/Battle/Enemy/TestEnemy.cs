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

        base.Attack();
    }
}
