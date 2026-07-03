using System;
using DG.Tweening;
using DG.Tweening.Core;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class GoodsIndicator : MonoBehaviour
{
    [Header("GoodsIndicator")]
    [SerializeField]
    private GoodsType _goodsType;

    [SerializeField]
    private string _prefix = string.Empty;

    [SerializeField]
    private string _suffix = string.Empty;

    private TMP_Text _goodsText;
    private SO_Goods _goods;
    private readonly char[] _goodsTextChar = new char[100];

    [Header("Tweening")]
    [SerializeField]
    private bool _useTween = true;

    [SerializeField, ShowIf(nameof(_useTween)), Min(0f)]
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
            UpdateText(x);
        };
    }

    private void Start()
    {
        _goods = GoodsManager.Instance.GetGoods(_goodsType);
        _displayGoods = _goods.Get();
        UpdateText(_displayGoods);
        _goods.OnValueChange += OnGoodsChange;
    }

    private void UpdateText(uint value)
    {
        Span<char> charSpan = _goodsTextChar;
        int offset = 0;

        if (
            !charSpan.TryAppend(ref offset, (_prefix ?? string.Empty).AsSpan())
            || !charSpan.TryAppend(ref offset, value, "N0")
            || !charSpan.TryAppend(ref offset, (_suffix ?? string.Empty).AsSpan())
        )
        {
            return;
        }

        _goodsText.SetCharArray(_goodsTextChar, 0, offset); // GC 할당 방지
    }

    private void OnGoodsChange(uint goods)
    {
        _indicateTweener?.Kill();
        _indicateTweener = null;

        if (!_useTween || _duration <= 0f)
        {
            _setter(goods);
            return;
        }

        // 재화 수가 n -> m 까지 쭉 오르는 연출
        _indicateTweener = DOTween.To(_getter, _setter, goods, _duration);
    }

    private void OnDestroy()
    {
        _indicateTweener?.Kill();

        if (_goods != null)
            _goods.OnValueChange -= OnGoodsChange;
    }
}
