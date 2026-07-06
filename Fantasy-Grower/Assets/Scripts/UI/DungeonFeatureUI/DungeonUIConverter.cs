using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 던전의 기본 UI를 통해 각각의 UI를 전환하는 역할
/// Canvas에 붙인다.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class DungeonUIConverter : MonoBehaviour
{
    public enum UIKind
    {
        Shop = -2,
        Skill = -1,
        None = 0,
        Equipment = 1,
        Dungeon = 2,
    }

    [System.Serializable]
    private struct UIKindToPanel
    {
        public UIKind Kind;
        public RectTransform Panel;
    }

    [System.Serializable]
    private class ButtonEventParam
    {
        public Button Button;
        public UIKind Kind;
    }

    [SerializeField]
    private UIKindToPanel[] _uiPanelKeyPair;

    private readonly Dictionary<UIKind, RectTransform> _uiPanelDic = new();

    [SerializeField]
    private float _convertDuration = 0.5f;

    [SerializeField]
    private ButtonEventParam[] _buttonEventParams;

    private Canvas _canvas; // 캔버스의 Size를 구하기 위한 변수

    private float _width;

    private UIKind _curUIKind = UIKind.None;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();

        foreach (var pair in _uiPanelKeyPair)
        {
            _uiPanelDic[pair.Kind] = pair.Panel;
            pair.Panel.gameObject.SetActive(false);
        }

        foreach (var eventParam in _buttonEventParams)
        {
            eventParam.Button.onClick.AddListener(() => Convert(eventParam.Kind));
        }
    }

    private void Start()
    {
        RectTransform canvasTransform = _canvas.transform as RectTransform;

        _width = canvasTransform.rect.width;
    }

    private void Convert(UIKind kind)
    {
        int compare = ((int)kind).CompareTo((int)_curUIKind);

        // 이미 있는 곳을 한 번 더 눌렀을 때
        if (compare == 0)
            return;

        if (
            !_uiPanelDic.TryGetValue(_curUIKind, out RectTransform curPanel)
            || !_uiPanelDic.TryGetValue(kind, out RectTransform newPanel)
        )
            return;

        curPanel.DOComplete();
        curPanel
            .DOAnchorPosX(_width * -compare, _convertDuration)
            .SetEase(Ease.OutQuint)
            .SetRecyclable(true)
            .OnComplete(() => curPanel.gameObject.SetActive(false));
        newPanel.DOComplete();
        newPanel.gameObject.SetActive(true);
        newPanel
            .DOAnchorPosX(0, _convertDuration)
            .From(new Vector2(_width * compare, 0))
            .SetEase(Ease.OutQuint)
            .SetRecyclable(true);

        _curUIKind = kind;
    }
}
