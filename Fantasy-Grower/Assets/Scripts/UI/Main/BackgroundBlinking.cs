using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타이틀 화면의 배경을 깜빡이게 하는 클래스입니다.
/// </summary>
public class BackgroundBlinking : MonoBehaviour
{
    [SerializeField]
    private Image _background; // 배경

    private Tween _slowBlinkTween;
    private Tween _fastBlinkTween;

    private void Start()
    {
        // 명도만 조절하고 투명도(Alpha)는 1로 고정하기 위한 색상 캐싱
        Color slowBlinkColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        Color fastBlinkColor = new Color(0.4f, 0.4f, 0.4f, 1f);

        // Tween을 단 한 번만 생성하고, 자동 파괴(AutoKill)를 막아 캐싱합니다.
        _slowBlinkTween = _background
            .DOColor(slowBlinkColor, 2f)
            .From(Color.white)
            .SetAutoKill(false) // 트윈 재사용을 위한 핵심 설정
            .SetLink(gameObject)
            .Pause();

        _fastBlinkTween = _background
            .DOColor(fastBlinkColor, 0.1f)
            .From(Color.white)
            .SetLoops(4, LoopType.Yoyo)
            .SetAutoKill(false) // 트윈 재사용을 위한 핵심 설정
            .SetLink(gameObject)
            .Pause();

        StartCoroutine(BlinkStateManagement());
    }

    private IEnumerator BlinkStateManagement()
    {
        while (true)
        {
            // 루프 수만 동적으로 갱신 후 캐싱된 트윈 재시작
            _slowBlinkTween.SetLoops(Random.Range(2, 6), LoopType.Yoyo);
            _slowBlinkTween.Restart();

            // GC 할당 제로(Zero)를 위한 while 대기
            while (_slowBlinkTween.IsPlaying())
                yield return null;

            _fastBlinkTween.Restart();

            while (_fastBlinkTween.IsPlaying())
                yield return null;
        }
    }

    private void OnDestroy()
    {
        // SetAutoKill(false)로 캐싱한 트윈은 명시적으로 메모리에서 해제해 주는 것이 안전합니다.
        _slowBlinkTween?.Kill();
        _fastBlinkTween?.Kill();
    }

    private void OnValidate()
    {
        if (_background == null)
        {
            Debug.LogError("[BackgroundBlinking] Background Image가 지정되지 않았습니다.", this);
        }
    }
}
