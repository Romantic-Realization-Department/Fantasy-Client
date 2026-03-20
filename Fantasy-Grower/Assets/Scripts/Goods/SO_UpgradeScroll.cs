using UnityEngine;

[CreateAssetMenu(
    fileName = "UpgradeScroll",
    menuName = "ScriptableObjects/Goods/UpgradeScroll",
    order = 4
)]
public class SO_UpgradeScroll : SO_Goods
{
    protected override string GoodsName { get; } = "강화 스크롤";
}
