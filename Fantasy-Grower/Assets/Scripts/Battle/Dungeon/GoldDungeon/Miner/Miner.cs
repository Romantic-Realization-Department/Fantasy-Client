using UnityEngine;

/// <summary>
/// 선택한 플레이어의 공격력만을 이어받는 광부입니다. (스킬은 제외)
/// </summary>
public class Miner : Player
{
    [SerializeField]
    private GoldOre _goldOre;

    public override void Attack()
    {
        _goldOre.TakeDamage(AttackPower);
        base.Attack();
    }
}
