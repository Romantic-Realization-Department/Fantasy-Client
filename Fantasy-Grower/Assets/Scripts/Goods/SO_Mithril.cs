using UnityEngine;

[CreateAssetMenu(fileName = "Mithril", menuName = "ScriptableObjects/Goods/Mithril", order = 5)]
public class SO_Mithril : SO_Goods
{
    protected override string GoodsName { get; } = "미스릴";
}
