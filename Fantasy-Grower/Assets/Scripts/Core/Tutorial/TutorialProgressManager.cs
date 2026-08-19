using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TutorialTrigger
{
    [Tooltip("세이브 데이터에 완료 여부를 저장할 때 사용할 고유 ID")]
    public string tutorialID;

    [Tooltip("이 튜토리얼을 발동시킬 조건 (ScriptableObject)")]
    public TutorialConditionBase condition;

    [Tooltip("조건 달성 시 재생할 튜토리얼 시퀀스 데이터")]
    public TutorialSequenceData sequenceData;

    [HideInInspector]
    public bool isCompleted = false; // 임시 완료 플래그 (향후 세이브 시스템과 연동 시 프로퍼티로 교체 가능)
}

/// <summary>
/// 튜토리얼 조건들을 매 프레임 순회하며, 달성된 튜토리얼을 띄워주는 폴링(Polling) 전담 매니저.
/// </summary>
public class TutorialProgressManager : MonoBehaviour
{
    private static TutorialProgressManager _instance;
    public static TutorialProgressManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<TutorialProgressManager>();
                if (_instance == null)
                {
                    GameObject go = new("TutorialProgressManager");
                    _instance = go.AddComponent<TutorialProgressManager>();
                }
            }
            return _instance;
        }
    }

    [SerializeField]
    private List<TutorialTrigger> activeTriggers = new();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 튜토리얼 매니저의 종료 이벤트를 구독하여, 튜토리얼이 끝나면 자신을 다시 활성화함
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnTutorialSequenceCompleted += OnTutorialCompleted;
        }
    }

    private void OnDestroy()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnTutorialSequenceCompleted -= OnTutorialCompleted;
        }
    }

    private void OnTutorialCompleted()
    {
        // 튜토리얼이 끝났으므로 다시 조건 폴링을 재개함
        this.enabled = true;
    }

    private void Update()
    {
        // 최적화: for루프 순회 중 원소를 제거하기 위해 역순 순회
        for (int i = activeTriggers.Count - 1; i >= 0; i--)
        {
            var trigger = activeTriggers[i];

            // 이미 완료되었거나, 조건 데이터가 없는 경우는 스킵 (안전망)
            if (trigger.isCompleted || trigger.condition == null || trigger.sequenceData == null)
                continue;

            // 조건 달성 여부 검사
            if (trigger.condition.CheckCondition())
            {
                // 트리거 발동
                TriggerTutorial(trigger);

                // 리스트에서 제거하여 더 이상 검사하지 않음 + Swap-Pop (최적화)
                (activeTriggers[i], activeTriggers[^1]) = (activeTriggers[^1], activeTriggers[i]);
                activeTriggers.RemoveAt(activeTriggers.Count - 1);

                // [필수] 튜토리얼이 중복 실행되어 덮어씌워지는 것을 방지
                break;
            }
        }
    }

    private void TriggerTutorial(TutorialTrigger trigger)
    {
        trigger.isCompleted = true;
        // TODO: 세이브 시스템 연동 시 PlayerPrefs.SetInt(trigger.tutorialID, 1) 등 처리

        Debug.Log($"[TutorialProgressManager] 튜토리얼 조건 달성: {trigger.tutorialID}");

        // 튜토리얼 실행기에 재생을 위임
        TutorialManager.Instance.StartTutorialSequence(trigger.sequenceData.steps);

        // 재생이 시작되었으므로, 끝날 때까지 조건 폴링(Update)을 중단함
        this.enabled = false;
    }
}
