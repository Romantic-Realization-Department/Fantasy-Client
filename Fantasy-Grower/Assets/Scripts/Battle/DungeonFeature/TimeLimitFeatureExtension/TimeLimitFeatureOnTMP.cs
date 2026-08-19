using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TimeLimitFeature))]
public class TimeLimitFeatureOnTMP : MonoBehaviour
{
    private TimeLimitFeature _timeLimitFeature;

    [SerializeField]
    private TMP_Text _timeText;

    private readonly char[] _text = new char[10];

    private void Awake()
    {
        _timeLimitFeature = GetComponent<TimeLimitFeature>();
        _timeLimitFeature.OnUpdateUI += OnUpdate;
    }

    private void OnUpdate(float remainingTime)
    {
        Span<char> charSpan = _text;

        if (remainingTime.TryFormat(charSpan, out int charsWritten, "F1"))
        {
            _timeText.SetCharArray(_text, 0, charsWritten); // GC할당 방지
        }
    }

    private void OnDestroy()
    {
        _timeLimitFeature.OnUpdateUI -= OnUpdate;
    }

    private void OnValidate()
    {
        if (!_timeText)
        {
            Debug.LogError("Time Text가 할당되지 않았습니다!!!", this);
        }
    }
}
