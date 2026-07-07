using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 튜토리얼 스텝들을 큐에 담아 순차적으로 실행하는 매니저.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    private static TutorialManager _instance;
    public static TutorialManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<TutorialManager>();
                if (_instance == null)
                {
                    GameObject go = new("TutorialManager");
                    _instance = go.AddComponent<TutorialManager>();
                }
            }
            return _instance;
        }
    }

    private readonly Queue<TutorialStep> tutorialQueue = new();
    private TutorialStep currentStep;
    private float previousTimeScale = 1f;

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

    private void Update()
    {
        currentStep?.ExecuteStep();
    }

    /// <summary>
    /// TutorialSequence(인스펙터 데이터)로부터 스텝들을 전달받아 큐를 세팅하고 시작한다.
    /// </summary>
    public void StartTutorialSequence(List<TutorialStepData> stepsData)
    {
        // 현재 실행 중인 스텝이 없을 때(튜토리얼 진입 시)에만 타임스케일을 캐싱
        if (currentStep == null)
        {
            previousTimeScale = Time.timeScale;
        }

        Time.timeScale = 0f;

        tutorialQueue.Clear();

        foreach (var data in stepsData)
        {
            TutorialStep step = CreateStepFromData(data);
            if (step != null)
            {
                tutorialQueue.Enqueue(step);
            }
        }

        StartNextStep();
    }

    /// <summary>
    /// 데이터를 파싱하여 실제 TutorialStep 객체(레고 블록)로 조립한다.
    /// </summary>
    private TutorialStep CreateStepFromData(TutorialStepData data)
    {
        switch (data.actionType)
        {
            case TutorialActionType.CLICK_UI:
                return new TutorialStep_ClickUI(data);
            case TutorialActionType.DIALOGUE_ONLY:
                return new TutorialStep_DialogueOnly(data);
            default:
                Debug.LogWarning(
                    $"[TutorialManager] 아직 구현되지 않은 스텝 타입입니다: {data.actionType}"
                );
                return null;
        }
    }

    public event System.Action OnTutorialSequenceCompleted;

    private void StartNextStep()
    {
        if (currentStep != null)
        {
            currentStep.OnStepCompleted -= StartNextStep;
            currentStep.ExitStep();
            currentStep = null;
        }

        if (tutorialQueue.Count > 0)
        {
            currentStep = tutorialQueue.Dequeue();
            currentStep.OnStepCompleted += StartNextStep;
            currentStep.EnterStep();
        }
        else
        {
            Time.timeScale = previousTimeScale;
            Debug.Log("[TutorialManager] 모든 튜토리얼 스텝이 완료되었습니다.");

            // 튜토리얼 종료 이벤트 발화 (TutorialProgressManager 등에서 수신)
            OnTutorialSequenceCompleted?.Invoke();

            // TODO: 세이브 데이터에 완료 플래그 저장 로직 추가 예정
        }
    }
}
