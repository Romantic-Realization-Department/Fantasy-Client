using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 이벤트를 예고하는 텍스트를 띄우고, 호출합니다.
/// </summary>
public class AutoExecuteButtonAction : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _noticeText; // N초 후에 자동으로 XX합니다...

    [SerializeField, Tooltip("XX합니다 형식으로 작성")]
    private string _actionSentence;

    [SerializeField]
    private int _waitTime = 5;

    [SerializeField]
    private int _activeTime = 3;

    [SerializeField]
    private UnityEvent _onTimeout;

    private readonly char[] _buffer = new char[64];

    private float _reminingTime;
    private int _displayValue;
    private int DisplayValue
    {
        set
        {
            if (_displayValue != value)
            {
                _displayValue = value;

                if (_displayValue > _activeTime)
                    return;

                Span<char> charSpan = _buffer;
                int offset = 0;

                if (!_displayValue.TryFormat(charSpan[offset..], out int charsWritten))
                    return;
                offset += charsWritten;

                if (
                    !charSpan.TryAppend(ref offset, "초 뒤에 자동으로 ".AsSpan())
                    || !charSpan.TryAppend(ref offset, _actionSentence.AsSpan())
                    || !charSpan.TryAppend(ref offset, "...".AsSpan())
                )
                    return;

                _noticeText.SetCharArray(_buffer, 0, offset);
                _noticeText.gameObject.SetActive(true);

                if (_displayValue <= 0)
                {
                    _displayValue = 0;
                    _reminingTime = 0;

                    enabled = false;
                    _onTimeout.Invoke();
                }
            }
        }
    }

    private void Awake()
    {
        _reminingTime = _waitTime;
        _noticeText.gameObject.SetActive(false);
    }

    private void Update()
    {
        _reminingTime -= Time.deltaTime;
        DisplayValue = Mathf.CeilToInt(_reminingTime);
    }

    private void OnValidate()
    {
        if (!_noticeText)
        {
            Debug.LogError("Notice Text가 할당되지 않았습니다!!!", this);
        }

        if (_onTimeout == null)
        {
            Debug.LogError("On Timeout이벤트가 할당되지 않았습니다!!!", this);
        }
    }
}
