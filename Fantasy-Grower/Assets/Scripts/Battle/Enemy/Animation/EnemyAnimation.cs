using UnityEngine;

public class EnemyAnimation : EntityAnimation
{
    protected override void OnStateChanged(PlayerState state)
    {
        if (state == PlayerState.ATTACK)
        {
            _prefabs._anim.SetFloat(ATTACK_SPEED, _entity.AttackSpeed);
        }

        _prefabs.PlayAnimation(state, 0);
    }
}
