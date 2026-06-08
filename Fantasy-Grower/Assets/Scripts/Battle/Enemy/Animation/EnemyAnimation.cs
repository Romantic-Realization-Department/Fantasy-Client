using DG.Tweening;
using UnityEngine;

public class EnemyAnimation : EntityAnimation
{
    protected override void OnIdle()
    {
        _prefabs.PlayAnimation(PlayerState.IDLE, 0);
    }

    protected override void OnMove()
    {
        _prefabs.PlayAnimation(PlayerState.MOVE, 0);
    }

    protected override void OnAttack()
    {
        _prefabs._anim.SetFloat(ATTACK_SPEED, _entity.AttackSpeed);
        _prefabs.PlayAnimation(PlayerState.ATTACK, 0);
    }

    protected override void OnDamaged()
    {
        base.OnDamaged();

        HitEffectObjPool.Spawn(transform.position, Quaternion.identity);
    }

    protected override void OnDeath()
    {
        _prefabs.PlayAnimation(PlayerState.DEATH, 0);
    }
}
