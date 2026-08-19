using DG.Tweening;
using UnityEngine;

public enum PlayerClass
{
    Warrior = 0,
    Archer = 2,
    Mage = 4,
}

public class PlayerAnimation : EntityAnimation
{
    [Header("Player Setting")]
    [SerializeField]
    private PlayerClass _playerClass;

    protected override void OnIdle()
    {
        _prefabs.PlayAnimation(PlayerState.IDLE, 0);
    }

    protected override void OnAttack()
    {
        _prefabs._anim.SetFloat(ATTACK_SPEED, _entity.AttackSpeed); // 공격 속도에 따라 애니메이션 속도 조절
        _prefabs.PlayAnimation(PlayerState.ATTACK, (int)_playerClass); // 공격 애니메이션은 플레이어 클래스에 따라 다르게 재생
    }

    protected override void OnDamaged()
    {
        base.OnDamaged();
    }

    protected override void OnDeath()
    {
        _prefabs.PlayAnimation(PlayerState.DEATH, (int)_playerClass);
    }
}
