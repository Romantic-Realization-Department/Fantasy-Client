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
    [SerializeField]
    private DungeonType _dungeonType;

    [Header("웨이브 목록 (순서대로 진행)")]
    [SerializeField]
    private WaveData[] _waves;

    [Header("던전 클리어 보너스 보상")]
    [SerializeField]
    private uint _bonusGoldReward;

    [SerializeField]
    private uint _bonusXpReward;

    [Header("보스 던전 전용 보상")]
    [SerializeField]
    private uint _mithrilRewardAmount;

    public DungeonType DungeonType => _dungeonType;

    public WaveData[] Waves => _waves;

    public uint BonusGoldReward
    {
        get => _bonusGoldReward;
        set
        {
            if (!IsChangeable)
            {
                Debug.LogWarning($"던전 '{name}'의 보너스 골드 보상이 변경 불가능한 던전입니다.");
                return;
            }

            _bonusGoldReward = value;
        }
    }
    public uint BonusXpReward
    {
        get => _bonusXpReward;
        set
        {
            if (!IsChangeable)
            {
                Debug.LogWarning($"던전 '{name}'의 보너스 경험치 보상이 변경 불가능한 던전입니다.");
                return;
            }
            _bonusXpReward = value;
        }
    }

    public uint MithrilRewardAmount
    {
        get => _mithrilRewardAmount;
        set
        {
            if (!IsChangeable)
            {
                Debug.LogWarning($"던전 '{name}'의 미스릴 보상이 변경 불가능한 던전입니다.");
                return;
            }
            _mithrilRewardAmount = value;
        }
    }

    [field: SerializeField]
    public bool IsExistEnd { get; private set; }

    [field: SerializeField]
    public bool IsChangeable { get; private set; } // 골드 던전과 같이 성과에 따라 보상이 달라지는 던전은 true로 설정하여 보상 변경을 허용한다.
}
