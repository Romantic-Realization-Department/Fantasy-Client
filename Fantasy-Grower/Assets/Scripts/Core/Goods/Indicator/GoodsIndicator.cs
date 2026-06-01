using DG.Tweening;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class GoodsIndicator : MonoBehaviour
{
    private TMP_Text _goodsText;
    private SO_Goods _goods;

    [SerializeField]
    private GoodsType _goodsType;

    private void Awake()
    {
        _goodsText = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        _goods = GoodsManager.Instance.GetGoods(_goodsType);
        _goodsText.text = _goods.Get().ToString("#,##0");
        _goods.OnValueChange += OnGoodsChange;
    }

    private void OnGoodsChange(uint goods)
    {
        _goodsText.text = goods.ToString("#,##0");
        // TODO : 획득 애니메이션 효과 추가
    }

    private void OnDestroy()
    {
        _goods.OnValueChange -= OnGoodsChange;
    }
}
