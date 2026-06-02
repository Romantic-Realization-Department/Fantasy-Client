using UnityEngine;

public class MinerAnimation : EntityAnimation
{
    [Header("Miner Setting")]
    [SerializeField]
    private float _attackAnimationSpeed = 5;

    protected override void OnIdle()
    {
        _prefabs.PlayAnimation(PlayerState.IDLE, 0);
    }

    protected override void OnAttack()
    {
        _prefabs._anim.SetFloat(ATTACK_SPEED, _attackAnimationSpeed); // 광부는 고유값 사용
        _prefabs.PlayAnimation(PlayerState.ATTACK, 0);
    }
}
