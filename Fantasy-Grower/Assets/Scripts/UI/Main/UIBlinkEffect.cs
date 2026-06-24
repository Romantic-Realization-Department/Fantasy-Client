using UnityEngine;
using DG.Tweening;

/// <summary>
/// UI 요소를 주기적으로 깜빡이게(투명도 페이드 인/아웃) 만드는 클래스입니다.
/// 'Tap to Start' 텍스트나 알림 아이콘(N 뱃지) 등에 부착하여 사용합니다.
/// CanvasGroup 컴포넌트를 사용하여 하위 요소들까지 한 번에 투명도를 조절합니다.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIBlinkEffect : MonoBehaviour
{
    [SerializeField, Header("최소 투명도 (0: 완전 투명, 1: 완전 불투명)")]
    private float _minAlpha = 0.2f;

    [SerializeField, Header("최대 투명도")]
    private float _maxAlpha = 1.0f;

    [SerializeField, Header("한 번 깜빡이는데 걸리는 시간")]
    private float _duration = 0.8f;

    private Tween _blinkTween;
    private CanvasGroup _canvasGroup;

    private void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        
        // 시작 시 최대 투명도로 세팅
        _canvasGroup.alpha = _maxAlpha;

        // CanvasGroup의 alpha 값을 조절하여 투명도를 무한 반복 요요 형태로 애니메이션합니다.
        _blinkTween = _canvasGroup.DOFade(_minAlpha, _duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine) // 부드럽게 깜빡이도록 InOutSine 적용
            .SetLink(gameObject);    // 오브젝트 파괴 시 트윈 자동 종료 안전장치
    }

    private void OnDestroy()
    {
        _blinkTween?.Kill();
    }
}
