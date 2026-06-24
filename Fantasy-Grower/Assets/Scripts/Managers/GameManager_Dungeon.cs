using System.Collections.Generic;

public partial class GameManager
{
    // 스테이지 혹은 점수가 혼용되는 던전이 없으므로 단일 record로 관리
    private readonly Dictionary<DungeonType, long> _dungeonRecords = new();

    /// <summary>
    /// 특정 던전의 최고 기록(스테이지 또는 점수)을 갱신합니다.
    /// 기존 기록보다 높은 경우에만 저장됩니다.
    /// </summary>
    public void UpdateDungeonRecord(DungeonType type, long record)
    {
        if (!_dungeonRecords.ContainsKey(type) || record > _dungeonRecords[type])
        {
            _dungeonRecords[type] = record;

            // TODO: 추후 PlayerPrefs 또는 백엔드(서버) 저장 로직 추가
        }
    }

    /// <summary>
    /// 특정 던전의 최고 기록(스테이지 또는 점수)을 반환합니다.
    /// 기록이 없을 경우 0을 반환합니다.
    /// </summary>
    public long GetDungeonRecord(DungeonType type)
    {
        return _dungeonRecords.TryGetValue(type, out long record) ? record : 0;
    }
}
