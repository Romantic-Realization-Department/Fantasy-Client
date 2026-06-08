using System;
using DG.Tweening;
using DG.Tweening.Core;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class GoodsIndicator : MonoBehaviour
{
    [Header("GoodsIndicator")]
    [SerializeField]
    private GoodsType _goodsType;

    private TMP_Text _goodsText;
    private SO_Goods _goods;
    private readonly char[] _goodsTextChar = new char[100];

    [Header("Tweening")]
    [SerializeField]
    private float _duration = 0.3f;
    private uint _displayGoods = 0;
    private Tweener _indicateTweener;

    // 연출 최적화를 위한 캐싱용 변수
    private DOGetter<uint> _getter;
    private DOSetter<uint> _setter;

    private void Awake()
    {
        _goodsText = GetComponent<TMP_Text>();

        // 람다 캐싱
        _getter = () => _displayGoods;
        _setter = x =>
        {
            _displayGoods = x;

            Span<char> charSpan = _goodsTextChar;

            if (x.TryFormat(charSpan, out int charsWritten, "N0"))
            {
                _goodsText.SetCharArray(_goodsTextChar, 0, charsWritten); // GC할당 방지
            }
        };
    }

    private void Start()
    {
        _goods = GoodsManager.Instance.GetGoods(_goodsType);
        _displayGoods = _goods.Get();
        _goodsText.text = _displayGoods.ToString("#,##0");
        _goods.OnValueChange += OnGoodsChange;
    }

    private void OnGoodsChange(uint goods)
    {
        _indicateTweener?.Kill();

        // 재화 수가 n -> m 까지 쭉 오르는 연출
        _indicateTweener = DOTween.To(_getter, _setter, goods, _duration);
    }

    private void OnDestroy()
    {
        _goods.OnValueChange -= OnGoodsChange;
    }
}
