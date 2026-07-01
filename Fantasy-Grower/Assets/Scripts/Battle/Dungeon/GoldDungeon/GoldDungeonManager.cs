using System;
using System.Collections.Generic;
using UnityEngine;

public class GoldDungeonManager
    : DungeonManager<GoldDungeonData>,
        ITimeLimitedDungeon,
        IDungeonRewardRecorder
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

    // ─── 런타임 데이터 ──────────────────────────────────────────────
    private uint _goldInitialValue;
    private uint _mithrilInitialValue;

    public IReadOnlyList<RewardDisplayItem> GetRewardItems()
    {
        uint goldReward = GoodsManager.Instance.GetGoods(GoodsType.Gold).Get() - _goldInitialValue;
        uint mithrilReward =
            GoodsManager.Instance.GetGoods(GoodsType.Mithril).Get() - _mithrilInitialValue;

        return new[]
        {
            RewardDisplayItemFactory.Goods(GoodsType.Gold, goldReward),
            RewardDisplayItemFactory.Goods(GoodsType.Mithril, mithrilReward),
        };
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

    protected override void Clear()
    {
        SceneChanger.SceneLoaded -= OnSceneLoaded;
    }

    protected override void OnClearDungeon()
    {
        base.OnClearDungeon();

        // 획득한 골드량(점수) 기록 갱신
        if (_currentDungeonData != null)
        {
            uint goldReward =
                GoodsManager.Instance.GetGoods(GoodsType.Gold).Get() - _goldInitialValue;
            GameManager.Instance.UpdateDungeonRecord(_currentDungeonData.DungeonType, goldReward);
        }
    }
}
