using System;

/// <summary>
/// 모든 튜토리얼 스텝의 기본이 되는 추상 클래스.
/// </summary>
public abstract class TutorialStep
{
    /// <summary>
    /// 해당 스텝의 목표가 달성되었음을 매니저에게 알리는 이벤트
    /// </summary>
    public event Action OnStepCompleted;

    /// <summary>
    /// 스텝 진입 시 최초 1회 호출된다. (UI 차단, 가이드 출력 등)
    /// </summary>
    public abstract void EnterStep();

    /// <summary>
    /// 프레임 단위의 조건 검사가 필요한 경우 재정의하여 사용한다.
    /// 이벤트 기반으로만 동작한다면 비워두어도 무방하다.
    /// </summary>
    public virtual void ExecuteStep() { }

    /// <summary>
    /// 스텝 종료 시 호출된다. (UI 차단 해제, 리스너 해제 등 정리 작업)
    /// </summary>
    public abstract void ExitStep();

    /// <summary>
    /// 자식 클래스에서 목표 달성 시 호출하여 다음 스텝으로 넘어가게 한다.
    /// </summary>
    protected void CompleteStep()
    {
        OnStepCompleted?.Invoke();
    }
}
