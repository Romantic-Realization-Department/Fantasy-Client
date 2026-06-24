using DG.Tweening;
using UnityEngine;

/// <summary>
/// UI 요소의 크기를 주기적으로 변경(숨쉬는 듯한 효과)하는 클래스입니다.
/// 픽셀 아트가 깨지지 않도록 Scale 대신 RectTransform의 SizeDelta(너비/높이)를 조절합니다.
/// 9-Slice가 적용된 이미지에 사용해야 테두리 원본 픽셀이 왜곡되지 않습니다.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIPulseEffect : MonoBehaviour
{
    [SerializeField, Header("늘어날 가로/세로 픽셀 크기 (예: X: 10, Y: 10)")]
    private Vector2 _expandSize = new Vector2(10f, 10f);

    [SerializeField, Header("한 번 커지거나 작아지는데 걸리는 시간")]
    private float _duration = 1.0f;

    private Tween _pulseTween;
    private RectTransform _rectTransform;

    private void Start()
    {
        _rectTransform = (RectTransform)transform; // GetComponent 대신 빠른 다운캐스팅 활용

        // 현재 너비/높이를 기준으로 목표 크기를 계산합니다. (원본 픽셀 훼손 방지)
        Vector2 targetSize = _rectTransform.sizeDelta + _expandSize;

        // Scale 대신 SizeDelta를 조절하여 가운데 영역만 자연스럽게 부풀립니다.
        _pulseTween = _rectTransform
            .DOSizeDelta(targetSize, _duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine) // 부드러운 가감속
            .SetLink(gameObject); // 안전 장치
    }

    private void OnDestroy()
    {
        _pulseTween?.Kill();
    }
}
