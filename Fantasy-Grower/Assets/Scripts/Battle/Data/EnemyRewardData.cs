using System;
using UnityEngine;

/// <summary>
/// 적 종류별 기본 처치 보상과 스테이지에 따른 복리 증가율을 정의합니다.
/// 같은 적 종류의 프리팹은 하나의 에셋을 공유합니다.
/// </summary>
[CreateAssetMenu(fileName = "EnemyRewardData", menuName = "Battle/EnemyRewardData")]
public class EnemyRewardData : ScriptableObject
{
    [Header("1스테이지 기본 보상")]
    public uint GoldAmount;
    public uint XpAmount;

    [Header("스테이지당 복리 증가율")]
    [SerializeField, Min(0f)]
    private float goldGrowthRatePerStage = 0.25f;

    [SerializeField, Min(0f)]
    private float xpGrowthRatePerStage = 0.4f;

    public uint CalculateGold(int stageNumber) =>
        CalculateScaledReward(GoldAmount, goldGrowthRatePerStage, stageNumber);

    public uint CalculateXp(int stageNumber) =>
        CalculateScaledReward(XpAmount, xpGrowthRatePerStage, stageNumber);

    private static uint CalculateScaledReward(uint baseReward, float growthRate, int stageNumber)
    {
        if (baseReward == 0)
            return 0;

        int growthCount = Mathf.Max(0, stageNumber - 1);
        double multiplier = Math.Pow(1d + Mathf.Max(0f, growthRate), growthCount);
        double scaledReward = baseReward * multiplier;

        if (double.IsNaN(scaledReward) || scaledReward >= uint.MaxValue)
            return uint.MaxValue;

        return (uint)Math.Round(scaledReward, MidpointRounding.AwayFromZero);
    }
}
