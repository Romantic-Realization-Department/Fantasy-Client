using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class AutoReturnBasicDungeon : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _noticeText; // N초 후에 자동으로 돌아갑니다...

    [SerializeField]
    private int _waitTime = 5;

    [SerializeField]
    private int _activeTime = 3;

    [SerializeField]
    private UnityEvent _onTimeout;

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

                _noticeText.SetText("Auto-returning in {0}s...", _displayValue);
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
