using System.Collections.Generic;

public interface IStageDungeon
{
    void SetStage(DungeonData dungeonData);
}

public interface ITimeLimitedDungeon
{
    /// <summary>
    /// 던전이 시간 제한이 있는 던전인지 여부를 반환합니다.
    /// </summary>
    bool IsTimeLimited { get; }

    /// <summary>
    /// 던전 시간을 반환합니다. (초 단위)
    /// </summary>
    float GetTimeLimitSeconds();

    /// <summary>
    /// 시간이 다 되었을 때 호출됩니다.
    /// </summary>
    void OnTimeFinished();
}

public interface IDungeonRewardRecorder
{
    /// <summary>
    /// 현재 던전 진행 상황에 따른 보상을 계산하여 반환합니다.
    /// </summary>
    IReadOnlyList<RewardDisplayItem> GetRewardItems();
}
