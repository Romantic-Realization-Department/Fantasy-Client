using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public enum DungeonType
{
    Basic,
    Gold,
    Weapon,
    Boss,
}

[System.Serializable]
public class DungeonClearReward
{
    [System.Serializable]
    private struct RewardEachType
    {
        public GoodsType type;
        public uint count;
    }

    [SerializeField]
    private RewardEachType[] _rewardEachTypes;

    private readonly Dictionary<GoodsType, uint> _rewardDic = new();

    public uint this[GoodsType type]
    {
        get
        {
            if (!_rewardDic.TryGetValue(type, out uint value))
            {
                foreach (RewardEachType rewardEachType in _rewardEachTypes)
                {
                    _rewardDic[rewardEachType.type] = rewardEachType.count;
                    if (type == rewardEachType.type)
                    {
                        value = rewardEachType.count;
                        break;
                    }
                }
            }

            return value;
        }
        set => _rewardDic[type] = value;
    }
}

/// <summary>
/// 던전 필수 구성 데이터 (던전 유형 + 클리어 보상).
/// DungeonManager에 연결하여 던전을 정의한다.
/// </summary>
public abstract class DungeonData : ScriptableObject
{
    [Header("던전 유형")]
    [SerializeField]
    private DungeonType _dungeonType;

    [SerializeField]
    private bool _hasTimeLimit;

    [SerializeField, ShowIf(nameof(_hasTimeLimit))]
    private float _timeLimitSeconds;

    [Header("던전 클리어 보너스 보상")]
    [SerializeField]
    private DungeonClearReward _dungeonClearReward;

    public DungeonType DungeonType => _dungeonType;

    public bool HasTimeLimit => _hasTimeLimit;

    public float TimeLimitSeconds => _timeLimitSeconds;

    public DungeonClearReward DungeonClearReward => _dungeonClearReward;
}
