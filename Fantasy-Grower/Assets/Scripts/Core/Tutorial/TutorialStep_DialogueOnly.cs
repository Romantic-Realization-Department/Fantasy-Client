using UnityEngine;

public class TutorialStep_DialogueOnly : TutorialStep
{
    private TutorialStepData _data;

    public TutorialStep_DialogueOnly(TutorialStepData data)
    {
        _data = data;
    }

    public override void EnterStep()
    {
        // DIALOGUE_ONLY라도 강조(구멍 뚫기)할 대상 UI가 지정되어 있다면 가져온다.
        // 단, 터치 통과는 TutorialUIOverlay.IsRaycastLocationValid에서 막아주므로 시각적 강조만 들어간다.
        RectTransform targetRect = null;
        if (_data.targetUI_ID != null)
        {
            targetRect = UIRegistry.Get(_data.targetUI_ID);
        }

        TutorialUIOverlay.Instance.ShowGuide(targetRect, _data);
        TutorialUIOverlay.Instance.OnOverlayClicked += OnOverlayClicked;
    }

    private void OnOverlayClicked()
    {
        if (TutorialUIOverlay.Instance.IsTextAnimating)
        {
            // 텍스트 출력 중이면 출력을 즉시 완료시킴
            TutorialUIOverlay.Instance.CompleteTextAnimation();
        }
        else
        {
            // 출력이 모두 끝난 상태면 다음 스텝으로 넘어감
            CompleteStep();
        }
    }

    public override void ExitStep()
    {
        if (TutorialUIOverlay.Instance != null)
        {
            TutorialUIOverlay.Instance.OnOverlayClicked -= OnOverlayClicked;
            TutorialUIOverlay.Instance.HideGuide();
        }
    }
}
