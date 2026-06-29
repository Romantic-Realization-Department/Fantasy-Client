using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 지정된 고유 ID의 UI 버튼을 찾아, 사용자가 클릭할 때까지 대기하는 튜토리얼 스텝
/// </summary>
public class TutorialStep_ClickUI : TutorialStep
{
    private TutorialStepData _data;
    private Button _targetButton;

    public TutorialStep_ClickUI(TutorialStepData data)
    {
        _data = data;
    }

    public override void EnterStep()
    {
        // 1. UIRegistry에서 문자열 ID로 대상 RectTransform을 찾음
        RectTransform targetRect = UIRegistry.Get(_data.targetUI_ID);

        if (targetRect == null)
        {
            Debug.LogError(
                $"[TutorialStep_ClickUI] 대상을 찾을 수 없습니다 (ID: {_data.targetUI_ID}). UIRegistrar가 안 붙어있는지 확인하세요."
            );
            CompleteStep(); // 에러 시 게임 진행이 막히지 않게 강제 통과
            return;
        }

        _targetButton = targetRect.GetComponent<Button>();
        if (_targetButton == null)
        {
            Debug.LogError(
                $"[TutorialStep_ClickUI] 대상 UI에 Button 컴포넌트가 없습니다 (ID: {_data.targetUI_ID})"
            );
            CompleteStep();
            return;
        }

        // 2. 오버레이에 가이드(마스킹 구멍, 화살표, 텍스트 패널) 표시 지시
        if (TutorialUIOverlay.Instance != null)
        {
            TutorialUIOverlay.Instance.ShowGuide(targetRect, _data);
        }

        // 3. 버튼 클릭 리스너 등록
        _targetButton.onClick.AddListener(OnTargetClicked);
    }

    private void OnTargetClicked()
    {
        CompleteStep();
    }

    public override void ExitStep()
    {
        // 리스너 안전하게 제거
        if (_targetButton != null)
        {
            _targetButton.onClick.RemoveListener(OnTargetClicked);
        }

        // 가이드 UI 화면에서 제거
        if (TutorialUIOverlay.Instance != null)
        {
            TutorialUIOverlay.Instance.HideGuide();
        }
    }
}
