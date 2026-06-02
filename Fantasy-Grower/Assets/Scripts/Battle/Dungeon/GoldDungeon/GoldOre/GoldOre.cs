using UnityEngine;

/// <summary>
/// 골드 던전에서 광부가 공격할 수 있는 광석입니다.
/// </summary>
public class GoldOre : MonoBehaviour
{
    private SO_Goods _gold;
    private SO_Goods _mithril;
    private GoldDungeonData _goldDungeonData;

    public void Init(GoldDungeonData goldDungeonData)
    {
        _gold = GoodsManager.Instance.GetGoods(GoodsType.Gold);
        _mithril = GoodsManager.Instance.GetGoods(GoodsType.Mithril);
        _goldDungeonData = goldDungeonData;
    }

    public void TakeDamage(int damage)
    {
        _gold.Increase((uint)(damage * _goldDungeonData.GoldPerDamage));

        if (Random.Range(0, 100f) < _goldDungeonData.MithrilDropChance)
        {
            _mithril.Increase(_goldDungeonData.MithrilDropAmount);
        }

        HitEffectObjPool.Spawn(transform.position, transform.rotation);
    }
}
