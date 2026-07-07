using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "ScriptableObjects/Goods/Level", order = 6)]
public class SO_Level : SO_Goods
{
    public const uint MAX_LEVEL = 20;
    protected override string GoodsName { get; } = "레벨";

    public override void Increase(uint amount)
    {
        if (amount == 0 || value >= MAX_LEVEL)
            return;

        // 어떤 호출 경로에서도 최대 레벨을 넘지 않도록 Level 자원 자체가 마지막 경계를 보장합니다.
        base.Increase(System.Math.Min(amount, MAX_LEVEL - value));
    }

    public override bool Decrease(uint amount)
    {
        Debug.LogWarning("레벨은 감소시킬 수 없습니다!!!");
        return false;
    }
}
