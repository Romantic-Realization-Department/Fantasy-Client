using System;

/// <summary>
/// 스테이지 관련 기능을 외부 UI나 시스템에 제공하기 위한 공통 인터페이스입니다.
/// </summary>
public interface IStageProvider
{
    /// <summary>
    /// 현재 스테이지 인덱스 (0부터 시작)
    /// </summary>
    int CurrentStageIndex { get; }

    bool IsStageFixed { get; }

    /// <summary>
    /// 스테이지가 변경될 때 호출되는 이벤트. 현재 인덱스를 전달합니다.
    /// </summary>
    event Action<int> OnStageChanged;

    event Action<bool> OnStageFixedChanged;

    /// <summary>
    /// 원하는 스테이지 인덱스로 강제 이동합니다.
    /// </summary>
    /// <param name="targetIndex">이동할 스테이지의 인덱스 (0부터 시작)</param>
    void JumpToStage(int targetIndex);

    void SetStageFixed(bool isFixed);
}
