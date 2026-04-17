using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "ScriptableObjects/Goods/Level", order = 6)]
public class SO_Level : SO_Goods
{
    public const uint MAX_LEVEL = 20;
    protected override string GoodsName { get; } = "레벨";

    public override bool Decrease(uint amount)
    {
        Debug.LogWarning("레벨은 감소시킬 수 없습니다!!!");
        return false;
    }
}
