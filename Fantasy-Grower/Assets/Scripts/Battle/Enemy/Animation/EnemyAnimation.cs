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
        if (_takeDamageTweener != null && _takeDamageTweener.IsPlaying())
            _takeDamageTweener.Kill();

        _takeDamageTweener = _spriteRenderer
            .DOColor(_takeDamageColor, 0.1f)
            .SetLoops(2, LoopType.Yoyo);
    }

    protected override void OnDeath()
    {
        _prefabs.PlayAnimation(PlayerState.DEATH, 0);
    }
}
