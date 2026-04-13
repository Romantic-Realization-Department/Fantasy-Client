using UnityEngine;

public enum DungeonType
{
    Basic,
    Gold,
    Weapon,
    Boss,
}

/// <summary>
/// 던전 전체 구성 데이터 (웨이브 목록 + 던전 유형 + 클리어 보상).
/// BattleManager에 연결하여 던전을 정의한다.
/// </summary>
[CreateAssetMenu(fileName = "DungeonData", menuName = "Battle/DungeonData")]
public class DungeonData : ScriptableObject
{
    [Header("던전 유형")]
    public DungeonType DungeonType;

    [Header("웨이브 목록 (순서대로 진행)")]
    public WaveData[] Waves;

    [Header("던전 클리어 보너스 보상")]
    public uint BonusGoldReward;
    public uint BonusXpReward;

    [Header("보스 던전 전용 보상")]
    public uint MithrilRewardAmount;
}
