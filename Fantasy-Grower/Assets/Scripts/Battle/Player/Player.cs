using System.Collections;
using UnityEngine;

public class Player : Entity
{
    protected override int MaxEntityCount => 100;

    public override void Attack()
    {
        // TODO : 플레이어 애니메이션 효과 적용

        base.Attack();
    }
}
