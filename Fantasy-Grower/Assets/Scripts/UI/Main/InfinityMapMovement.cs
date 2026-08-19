using UnityEngine;

/// <summary>
/// 두 개의 패널을 이어 붙여 배경이 가로로 무한 이동하는 것처럼 보이게 합니다.
/// </summary>
public class InfinityMapMovement : MonoBehaviour
{
    [SerializeField]
    private RectTransform _firstMap;

    [SerializeField]
    private RectTransform _secondMap;

    [SerializeField]
    private float _moveSpeed = 100;

    [SerializeField, Range(-1, 1)]
    private int _direction = -1;

    private float _lastMapWidth;
    private Vector2 _firstStartPosition;
    private bool _isInitialized;

    private void Awake()
    {
        if (_firstMap == null || _secondMap == null)
        {
            return;
        }

        _firstStartPosition = _firstMap.anchoredPosition;
        _isInitialized = true;
        RefreshLayoutIfNeeded(true);
    }

    private void Update()
    {
        if (_firstMap == null || _secondMap == null || _direction == 0)
        {
            return;
        }

        RefreshLayoutIfNeeded();

        _firstMap.anchoredPosition += new Vector2(_moveSpeed * _direction * Time.deltaTime, 0);
        AlignSecondMap();

        if (_secondMap.anchoredPosition.x * _direction >= 0)
        {
            (_firstMap, _secondMap) = (_secondMap, _firstMap);
            AlignSecondMap();
        }
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!isActiveAndEnabled || !_isInitialized || _firstMap == null || _secondMap == null)
        {
            return;
        }

        RefreshLayoutIfNeeded(true);
    }

    private void RefreshLayoutIfNeeded(bool force = false)
    {
        float mapWidth = GetMapWidth(_firstMap);
        if (!force && Mathf.Approximately(_lastMapWidth, mapWidth))
        {
            return;
        }

        _lastMapWidth = mapWidth;
        _firstMap.anchoredPosition = _firstStartPosition;
        AlignSecondMap();
    }

    private void AlignSecondMap()
    {
        float mapWidth = GetMapWidth(_firstMap);
        _secondMap.anchoredPosition = new Vector2(
            _firstMap.anchoredPosition.x - (mapWidth * _direction),
            _secondMap.anchoredPosition.y
        );
    }

    private static float GetMapWidth(RectTransform map)
    {
        return map.rect.width;
    }

    private void OnValidate()
    {
        if (_firstMap == null)
        {
            Debug.LogError("[InfinityMapMovement] First Map이 지정되지 않았습니다.", this);
        }

        if (_secondMap == null)
        {
            Debug.LogError("[InfinityMapMovement] Second Map이 지정되지 않았습니다.", this);
        }
    }
}
