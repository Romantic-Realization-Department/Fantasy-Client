using UnityEngine;

[CreateAssetMenu(fileName = "Gold", menuName = "ScriptableObjects/Goods/Gold", order = 1)]
public class SO_Gold : SO_Goods
{
    protected override string GoodsName { get; } = "골드";
}
