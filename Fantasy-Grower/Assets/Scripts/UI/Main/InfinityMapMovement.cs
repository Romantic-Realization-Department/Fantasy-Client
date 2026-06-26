using UnityEngine;

/// <summary>
/// 두 개의 패널을 이용해서 배경의 움직임을 무한 맵으로 만드는 클래스입니다.
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

    private void Update()
    {
        // 1. _firstMap은 지정된 방향으로 계속 이동
        _firstMap.anchoredPosition += new Vector2(_moveSpeed * _direction * Time.deltaTime, 0);

        // 2. _secondMap은 _firstMap의 꼬리를 정확히 물고 따라가도록 배치
        _secondMap.anchoredPosition = new Vector2(
            _firstMap.anchoredPosition.x - (_firstMap.sizeDelta.x * _direction),
            _secondMap.anchoredPosition.y
        );

        // 3. _secondMap이 화면 중앙(0)을 넘어설 때 스왑 처리
        if (_secondMap.anchoredPosition.x * _direction >= 0)
        {
            (_firstMap, _secondMap) = (_secondMap, _firstMap); // 참조 스왑

            // 스왑 직후, 이번 프레임 렌더링에 빈 공간이 보이지 않도록 즉시 꼬리에 다시 붙임
            _secondMap.anchoredPosition = new Vector2(
                _firstMap.anchoredPosition.x - (_firstMap.sizeDelta.x * _direction),
                _secondMap.anchoredPosition.y
            );
        }
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
