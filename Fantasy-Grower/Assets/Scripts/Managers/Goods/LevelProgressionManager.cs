using System;
using UnityEngine;

/// <summary>
/// 누적 XP를 기준으로 현재 레벨을 계산하고, 새로 오른 레벨만큼 SP를 지급합니다.
/// XP, Level, SP 사이의 진행 규칙만 담당하며 플레이어 전투 스탯 계산은 별도 컴포넌트가 처리합니다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(GoodsManager))]
public sealed class LevelProgressionManager : MonoBehaviour
{
    [SerializeField, Min(1)]
    [Tooltip("레벨이 1 오를 때 지급하는 스킬 포인트입니다.")]
    private int spPerLevel = 1;

    private SO_XP xp;
    private SO_Level level;
    private SO_SP sp;

    /// <summary>
    /// 실제 플레이 중 레벨이 상승했을 때 이전 레벨과 새 레벨을 전달합니다.
    /// 저장 데이터 동기화 과정에서는 중복 연출과 중복 보상을 막기 위해 호출하지 않습니다.
    /// </summary>
    public event Action<uint, uint> OnLevelUp;

    private void Start()
    {
        GoodsManager goodsManager = GetComponent<GoodsManager>();
        xp = goodsManager.GetGoods(GoodsType.XP) as SO_XP;
        level = goodsManager.GetGoods(GoodsType.Level) as SO_Level;
        sp = goodsManager.GetGoods(GoodsType.SP) as SO_SP;

        if (xp == null || level == null || sp == null)
        {
            Debug.LogError(
                "[LevelProgressionManager] GoodsManager에 XP, Level, SP 자원을 모두 등록해주세요.",
                this
            );
            return;
        }

        // 저장된 XP와 Level의 로드 순서가 달라도 시작 시 한 번 정합성을 맞춥니다.
        // SP는 별도로 저장되는 값이므로 로드 동기화 중에는 다시 지급하지 않습니다.
        SynchronizeLevel(xp.Get(), false);
        xp.OnValueChange += HandleXpChanged;
    }

    private void OnDestroy()
    {
        if (xp != null)
            xp.OnValueChange -= HandleXpChanged;
    }

    /// <summary>
    /// 누적 XP가 어느 레벨에 해당하는지 계산합니다. XP는 소비하지 않습니다.
    /// NeedXpTable의 0번 값은 1레벨에서 2레벨로 오르는 데 필요한 XP입니다.
    /// </summary>
    public static uint CalculateLevel(uint totalXp)
    {
        uint calculatedLevel = 1;
        uint remainingXp = totalXp;

        while (calculatedLevel < SO_Level.MAX_LEVEL)
        {
            uint requiredXp = SO_XP.NeedXpTable[(int)calculatedLevel - 1];
            if (remainingXp < requiredXp)
                break;

            remainingXp -= requiredXp;
            calculatedLevel++;
        }

        return calculatedLevel;
    }

    private void HandleXpChanged(uint currentXp)
    {
        SynchronizeLevel(currentXp, true);
    }

    private void SynchronizeLevel(uint currentXp, bool grantReward)
    {
        uint previousLevel = level.Get();
        uint targetLevel = CalculateLevel(currentXp);

        // Level 에셋의 초기값이 0이어도 게임 내 레벨은 항상 1부터 시작합니다.
        // 저장된 Level이 XP보다 앞서 있는 경우에는 진행도를 되돌리지 않습니다.
        if (targetLevel <= previousLevel)
            return;

        uint gainedLevels = targetLevel - previousLevel;
        if (grantReward && !TryGrantSp(gainedLevels))
            return;

        level.Increase(gainedLevels);

        if (grantReward)
            OnLevelUp?.Invoke(previousLevel, level.Get());
    }

    private bool TryGrantSp(uint gainedLevels)
    {
        ulong reward = (ulong)gainedLevels * (uint)spPerLevel;
        ulong result = sp.Get() + reward;

        // SP와 Level 중 하나만 변경되는 부분 성공을 막기 위해 먼저 전체 연산 가능 여부를 검증합니다.
        if (reward > uint.MaxValue || result > uint.MaxValue)
        {
            Debug.LogError(
                "[LevelProgressionManager] SP가 uint 범위를 초과하여 레벨업을 처리할 수 없습니다.",
                this
            );
            return false;
        }

        sp.Increase((uint)reward);
        return true;
    }

    private void OnValidate()
    {
        spPerLevel = Mathf.Max(1, spPerLevel);
    }
}
