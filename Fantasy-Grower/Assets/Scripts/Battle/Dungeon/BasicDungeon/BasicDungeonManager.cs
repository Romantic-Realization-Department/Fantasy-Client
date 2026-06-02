using System;
using System.Collections;
using UnityEngine;

// TODO : IStageDungeon 인터페이스로 확장
public interface IStageDungeon
{
    void SetStage(DungeonData dungeonData);
}

/// <summary>
/// 전투 루프의 중심 오케스트레이터.
/// 던전 시작 → 웨이브 스폰 → 전투 → 클리어/사망 상태 전환을 관리한다.
/// UI 레이어는 OnStateChanged / OnWaveChanged 이벤트를 구독하여 화면을 갱신한다.
/// </summary>
public class BasicDungeonManager : DungeonManager<BasicDungeonData>, IStageDungeon
{
    public enum BasicDungeonState
    {
        Idle,
        WaveStart,
        Fighting,
        WaveCleared,
    }

    // ─── Inspector 연결 ──────────────────────────────────────────
    [SerializeField, Tooltip("플레이어 캐릭터")]
    private Player _player;

    [SerializeField, Tooltip("웨이브 컨트롤러")]
    private WaveController _waveController;

    [SerializeField, Tooltip("던전 데이터")]
    private BasicDungeonData _basicDungeonData;

    [Header("스폰 설정")]
    [SerializeField, Tooltip("스폰 시작 지점")]
    private Transform _spawnPoint;

    [SerializeField, Tooltip("스폰 지점 간격")]
    private float _spawnPointInterval = 1.0f;

    // ─── 런타임 상태 ─────────────────────────────────────────────
    private BasicDungeonState _basicDungeonState = BasicDungeonState.Idle;
    private int _currentWaveIndex;

    // ─── UI 알림 이벤트 ───────────────────────────────────────────
    /// <summary>상태가 변경될 때마다 현재 상태를 전달한다.</summary>
    public event Action<BasicDungeonState> OnBasicDungeonStateChanged;

    /// <summary>새 웨이브가 시작될 때 웨이브 번호(0-based)를 전달한다.</summary>
    public event Action<int> OnWaveChanged;

    // ─── 유니티 라이프사이클 직속 ──────────────────────────────────────
    protected override void Init()
    {
        WaveController.OnAllEnemiesDead += HandleAllEnemiesDead;
        _player.OnDied += HandlePlayerDied;
        SceneChanger.SceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded()
    {
        StartDungeon(_basicDungeonData);
    }

    protected override void Clear()
    {
        WaveController.OnAllEnemiesDead -= HandleAllEnemiesDead;
        _player.OnDied -= HandlePlayerDied;
        SceneChanger.SceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 던전 시작 시 초기 웨이브 인덱스를 0으로 설정한다. 이후 상태 머신이 웨이브 시작 → 전투 → 클리어 → 다음 웨이브 시작 순으로 진행한다.
    /// </summary>
    /// <param name="dungeonData"></param>
    protected override void StartDungeonInternal(BasicDungeonData dungeonData)
    {
        _currentWaveIndex = 0;
    }

    /// <summary>플레이어 사망 후 던전을 처음부터 재시도한다.</summary>
    public override void RetryDungeon()
    {
        _waveController.Clear();
        _player.ResetHp();
        _currentWaveIndex = 0;
        base.RetryDungeon();
    }

    // ─── 상태 머신 ────────────────────────────────────────────────
    private void TransitionTo(BasicDungeonState newState)
    {
        if (_delayedTransitionCoroutine != null)
        {
            StopCoroutine(_delayedTransitionCoroutine);
            _delayedTransitionCoroutine = null;
        }

        _basicDungeonState = newState;
        OnBasicDungeonStateChanged?.Invoke(_basicDungeonState);
        Debug.Log($"[BasicDungeonManager] 상태 전환: {newState}");

        switch (_basicDungeonState)
        {
            case BasicDungeonState.WaveStart:
                OnWaveStart();
                break;
            case BasicDungeonState.Fighting:
                OnFighting();
                break;
            case BasicDungeonState.WaveCleared:
                OnWaveCleared();
                break;
        }
    }

    private void OnWaveStart()
    {
        Debug.Log(
            $"[BasicDungeonManager] 웨이브 {_currentWaveIndex + 1} / {_currentDungeonData.Waves.Length} 시작"
        );
        OnWaveChanged?.Invoke(_currentWaveIndex);

        WaveData wave = _currentDungeonData.Waves[_currentWaveIndex];
        _waveController.SpawnWave(wave, _spawnPoint, _spawnPointInterval);

        TransitionTo(BasicDungeonState.Fighting);
    }

    private void OnFighting() { }

    private void HandleAllEnemiesDead()
    {
        TransitionTo(BasicDungeonState.WaveCleared);
    }

    private void OnWaveCleared()
    {
        _currentWaveIndex++;

        if (_currentWaveIndex >= _currentDungeonData.Waves.Length)
        {
            TransitionTo(DungeonState.Cleared);
        }
        else
        {
            _delayedTransitionCoroutine = StartCoroutine(
                DelayedTransition(BasicDungeonState.WaveStart, _currentDungeonData.NextWaveDelay)
            );
        }
    }

    protected override void OnPrepareDungeon()
    {
        TransitionTo(DungeonState.Running);
    }

    protected override void OnRunningDungeon()
    {
        TransitionTo(BasicDungeonState.WaveStart);
    }

    protected override void OnClearDungeon()
    {
        Debug.Log("[BasicDungeonManager] 던전 클리어!");

        GoodsManager
            .Instance.GetGoods(GoodsType.Gold)
            .Increase(_currentDungeonData.DungeonClearReward.GoldReward);
        GoodsManager
            .Instance.GetGoods(GoodsType.XP)
            .Increase(_currentDungeonData.DungeonClearReward.XpReward);
        GoodsManager
            .Instance.GetGoods(GoodsType.Mithril)
            .Increase(_currentDungeonData.DungeonClearReward.MithrilReward);
    }

    private void HandlePlayerDied(Entity entity)
    {
        if (entity != _player)
            return;

        _waveController.Clear();
        TransitionTo(DungeonState.Failed);
    }

    protected override void OnFailDungeon()
    {
        Debug.Log("[BasicDungeonManager] 플레이어 사망.");
        // UI에서 OnStateChanged(PlayerDead)를 받아 재시도/종료 화면을 표시한다.
    }

    // ─── 내부 유틸 ────────────────────────────────────────────────
    private IEnumerator DelayedTransition(BasicDungeonState next, float delay)
    {
        yield return YieldInstructionCache.WaitForSeconds(delay);
        TransitionTo(next);
    }

    // ─── IStageDungeon 구현 ─────────────────────────────────────────────
    public void SetStage(DungeonData dungeonData)
    {
        if (dungeonData is BasicDungeonData basicData)
        {
            StartDungeon(basicData);
        }
        else
        {
            Debug.LogError(
                $"[BasicDungeonManager] 잘못된 던전 데이터 타입: {dungeonData.GetType().Name}"
            );
        }
    }
}
