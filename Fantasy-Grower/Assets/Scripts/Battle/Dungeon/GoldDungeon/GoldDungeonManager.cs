using System;
using UnityEngine;

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

public interface IDungeonRewardProvider
{
    /// <summary>
    /// 현재 던전 진행 상황에 따른 보상을 계산하여 반환합니다.
    /// </summary>
    DungeonClearReward GetReward();
}

public class GoldDungeonManager
    : DungeonManager<GoldDungeonData>,
        ITimeLimitedDungeon,
        IDungeonRewardProvider
{
    // ─── ITimeLimitedDungeon 구현 ─────────────────────────────────────────────
    public bool IsTimeLimited => _currentDungeonData.HasTimeLimit;

    public float GetTimeLimitSeconds() => _currentDungeonData.TimeLimitSeconds;

    public void OnTimeFinished()
    {
        // 시간 초과 던전 종료 처리
        Debug.Log("시간 초과! 던전 종료 처리");
        TransitionTo(DungeonState.Cleared);
    }

    // ─── 초기 던전 데이터 ─────────────────────────────────────────────
    [SerializeField]
    private GoldDungeonData _goldDungeonData;

    [SerializeField]
    private GoldOre _goldOre;

    [SerializeField]
    private GameObject _dungeonClearUI;

    // ─── 런타임 데이터 ──────────────────────────────────────────────
    private uint _goldInitialValue;
    private uint _mithrilInitialValue;

    public DungeonClearReward GetReward()
    {
        uint goldReward = GoodsManager.Instance.GetGoods(GoodsType.Gold).Get() - _goldInitialValue;
        uint mithrilReward =
            GoodsManager.Instance.GetGoods(GoodsType.Mithril).Get() - _mithrilInitialValue;
        DungeonClearReward dungeonClearReward = new DungeonClearReward();

        dungeonClearReward[GoodsType.Gold] = goldReward;
        dungeonClearReward[GoodsType.Mithril] = mithrilReward;

        return dungeonClearReward;
    }

    protected override void Init()
    {
        SceneChanger.SceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded()
    {
        StartDungeon(_goldDungeonData);
    }

    protected override void StartDungeonInternal(GoldDungeonData dungeonData)
    {
        _goldOre.Init(dungeonData);
        _goldInitialValue = GoodsManager.Instance.GetGoods(GoodsType.Gold).Get();
        _mithrilInitialValue = GoodsManager.Instance.GetGoods(GoodsType.Mithril).Get();
    }

    public override void RetryDungeon()
    {
        _goldOre.Init(_currentDungeonData);
        _goldInitialValue = GoodsManager.Instance.GetGoods(GoodsType.Gold).Get();
        _mithrilInitialValue = GoodsManager.Instance.GetGoods(GoodsType.Mithril).Get();

        base.RetryDungeon();
    }

    protected override void OnPrepareDungeon()
    {
        TransitionTo(DungeonState.Running);
    }

    protected override void OnClearDungeon()
    {
        _dungeonClearUI.SetActive(true);
    }

    protected override void Clear()
    {
        SceneChanger.SceneLoaded -= OnSceneLoaded;
    }
}
