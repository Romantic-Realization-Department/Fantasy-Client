using UnityEngine;

/// <summary>
/// 골드 던전에서 광부가 공격할 수 있는 광석입니다.
/// </summary>
public class GoldOre : MonoBehaviour
{
    [SerializeField, Tooltip("미스릴이 드롭될 확률")]
    private float _mithrilDropPercent = 1f;

    [SerializeField, Tooltip("미스릴이 한 번에 드롭될 개수")]
    private uint _mithrilDropCount = 1;

    private SO_Goods _gold;
    private SO_Goods _mithril;

    private void Start()
    {
        _gold = GoodsManager.Instance.GetGoods(GoodsType.Gold);
        _mithril = GoodsManager.Instance.GetGoods(GoodsType.Mithril);
    }

    public void TakeDamage(int damage)
    {
        _gold.Increase((uint)damage);

        if (Random.Range(0, 100f) < _mithrilDropPercent)
        {
            _mithril.Increase(_mithrilDropCount);
        }

        HitEffectObjPool.Spawn(transform.position, transform.rotation);
    }
}
