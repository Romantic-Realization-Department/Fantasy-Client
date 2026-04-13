using UnityEngine;

[CreateAssetMenu(fileName = "SP", menuName = "ScriptableObjects/Goods/SP", order = 3)]
public class SO_SP : SO_Goods
{
    protected override string GoodsName { get; } = "스킬 포인트";
}
