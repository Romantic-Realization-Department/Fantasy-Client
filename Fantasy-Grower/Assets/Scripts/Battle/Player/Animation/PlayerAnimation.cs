using UnityEngine;

public enum PlayerClass
{
    Warrior = 0,
    Archer = 2,
    Mage = 4,
}

public class PlayerAnimation : EntityAnimation
{
    [SerializeField]
    private PlayerClass _playerClass;

    protected override void OnStateChanged(PlayerState state)
    {
        if (state == PlayerState.ATTACK)
        {
            _prefabs._anim.SetFloat(ATTACK_SPEED, _entity.AttackSpeed);
            _prefabs.PlayAnimation(state, (int)_playerClass);
        }
        else
        {
            _prefabs.PlayAnimation(state, 0);
        }
    }
}
