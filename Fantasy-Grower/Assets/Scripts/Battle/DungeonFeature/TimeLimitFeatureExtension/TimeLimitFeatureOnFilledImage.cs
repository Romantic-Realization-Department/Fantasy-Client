using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TimeLimitFeature))]
public class TimeLimitFeatureOnFilledImage : MonoBehaviour
{
    private TimeLimitFeature _timeLimitFeature;

    [SerializeField]
    private Slider _slider;

    private float _timeLimitInitial;

    private void Awake()
    {
        _timeLimitFeature = GetComponent<TimeLimitFeature>();
        _timeLimitFeature.OnStartedUI += OnStarted;
        _timeLimitFeature.OnUpdateUI += OnUpdate;
    }

    private void OnStarted(ITimeLimitedDungeon timeLimitedDungeon)
    {
        _timeLimitInitial = timeLimitedDungeon.GetTimeLimitSeconds();
    }

    private void OnUpdate(float remainingTime)
    {
        _slider.value = remainingTime / _timeLimitInitial;
    }

    private void OnDestroy()
    {
        _timeLimitFeature.OnStartedUI -= OnStarted;
        _timeLimitFeature.OnUpdateUI -= OnUpdate;
    }

    private void OnValidate()
    {
        if (!_slider)
        {
            Debug.LogError($"Slider가 할당되지 않았습니다!!!", this);
        }
    }
}
