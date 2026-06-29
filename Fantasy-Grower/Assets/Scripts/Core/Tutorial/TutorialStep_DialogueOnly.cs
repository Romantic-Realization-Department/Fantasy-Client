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
        // 타겟을 지정하지 않고 오버레이 중앙에 가이드를 띄움
        TutorialUIOverlay.Instance.ShowGuide(null, _data);
        TutorialUIOverlay.Instance.OnOverlayClicked += CompleteStep;
    }

    public override void ExitStep()
    {
        if (TutorialUIOverlay.Instance != null)
        {
            TutorialUIOverlay.Instance.OnOverlayClicked -= CompleteStep;
            TutorialUIOverlay.Instance.HideGuide();
        }
    }
}
