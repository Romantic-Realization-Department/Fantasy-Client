using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    Idle,
    WaveStart,
    Fighting,
    WaveCleared,
    DungeonCleared,
    PlayerDead,
}

/// <summary>
/// 전투 루프의 중심 오케스트레이터.
/// 던전 시작 → 웨이브 스폰 → 전투 → 클리어/사망 상태 전환을 관리한다.
/// UI 레이어는 OnStateChanged / OnWaveChanged 이벤트를 구독하여 화면을 갱신한다.
/// </summary>
public class BattleManager : MonoBehaviour
{
    private static BattleManager _instance;
    public static BattleManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<BattleManager>();
                if (_instance == null)
                {
                    Debug.LogError("[BattleManager] 씬에 BattleManager가 존재하지 않습니다.");
                }
            }
            return _instance;
        }
    }

    // ─── Inspector 연결 ──────────────────────────────────────────
    [SerializeField, Tooltip("플레이어 캐릭터")]
    private Player player;

    [SerializeField, Tooltip("웨이브 컨트롤러")]
    private WaveController waveController;

    [Header("스폰 설정")]
    [SerializeField, Tooltip("스폰 시작 지점")]
    private Transform spawnPoint;

    [SerializeField, Tooltip("스폰 지점 간격")]
    private float spawnPointInterval = 1.0f;

    // ─── 런타임 상태 ─────────────────────────────────────────────
    private BattleState state = BattleState.Idle;
    private int currentWaveIndex;
    private Coroutine _delayedTransitionCoroutine;

    public DungeonData DungeonData { get; set; }

    // ─── UI 알림 이벤트 ───────────────────────────────────────────
    /// <summary>상태가 변경될 때마다 발화된다. UI 패널 전환에 사용한다.</summary>
    public event Action<BattleState> OnStateChanged;

    /// <summary>새 웨이브가 시작될 때 웨이브 번호(0-based)를 전달한다.</summary>
    public event Action<int> OnWaveChanged;

    /// <summary>던전이 클리어되었을 때 발화된다. 던전 번호를 전달한다.</summary>
    public event Action OnDungeonCleared;

    // ─── 유니티 라이프사이클 ──────────────────────────────────────
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("[BattleManager] 씬에 BattleManager가 2개 이상 존재합니다.");
            Destroy(this);
            return;
        }
        _instance = this;

        WaveController.OnAllEnemiesDead += HandleAllEnemiesDead;
        player.OnDied += HandlePlayerDied;
    }

    private void OnDestroy()
    {
        WaveController.OnAllEnemiesDead -= HandleAllEnemiesDead;
        player.OnDied -= HandlePlayerDied;

        if (_instance == this)
        {
            _instance = null;
        }
    }

    // ─── 공개 API (UI 버튼에서 호출) ─────────────────────────────
    /// <summary>던전을 시작한다.</summary>
    public void StartDungeon()
    {
        if (DungeonData == null)
        {
            Debug.LogError("[BattleManager] DungeonData가 연결되지 않았습니다.");
            return;
        }

        if (DungeonData.DungeonType == DungeonType.Gold)
        {
            Debug.Log("[BattleManager] 골드 던전은 미니게임 씬으로 전환해야 합니다.");
            // TODO: SceneManager.LoadScene("GoldDungeonScene");
            return;
        }

        currentWaveIndex = 0;
        TransitionTo(BattleState.WaveStart);
    }

    /// <summary>플레이어 사망 후 던전을 처음부터 재시도한다.</summary>
    public void RetryDungeon()
    {
        waveController.Clear();
        player.ResetHp();
        currentWaveIndex = 0;
        TransitionTo(BattleState.WaveStart);
    }

    // ─── 상태 머신 ────────────────────────────────────────────────
    private void TransitionTo(BattleState newState)
    {
        if (_delayedTransitionCoroutine != null)
        {
            StopCoroutine(_delayedTransitionCoroutine);
            _delayedTransitionCoroutine = null;
        }

        state = newState;
        OnStateChanged?.Invoke(state);
        Debug.Log($"[BattleManager] 상태 전환: {newState}");

        switch (state)
        {
            case BattleState.WaveStart:
                EnterWaveStart();
                break;
            case BattleState.Fighting:
                EnterFighting();
                break;
            case BattleState.WaveCleared:
                EnterWaveCleared();
                break;
            case BattleState.DungeonCleared:
                EnterDungeonCleared();
                break;
            case BattleState.PlayerDead:
                EnterPlayerDead();
                break;
        }
    }

    private void EnterWaveStart()
    {
        Debug.Log(
            $"[BattleManager] 웨이브 {currentWaveIndex + 1} / {DungeonData.Waves.Length} 시작"
        );
        OnWaveChanged?.Invoke(currentWaveIndex);

        WaveData wave = DungeonData.Waves[currentWaveIndex];
        waveController.SpawnWave(wave, spawnPoint, spawnPointInterval);

        TransitionTo(BattleState.Fighting);
    }

    private void EnterFighting() { }

    private void HandleAllEnemiesDead()
    {
        TransitionTo(BattleState.WaveCleared);
    }

    private void EnterWaveCleared()
    {
        currentWaveIndex++;

        if (currentWaveIndex >= DungeonData.Waves.Length)
        {
            TransitionTo(BattleState.DungeonCleared);
        }
        else
        {
            _delayedTransitionCoroutine = StartCoroutine(
                DelayedTransition(BattleState.WaveStart, 1.5f)
            );
        }
    }

    private void EnterDungeonCleared()
    {
        Debug.Log("[BattleManager] 던전 클리어!");

        GoodsManager.Instance.GetGoods(GoodsType.Gold).Increase(DungeonData.BonusGoldReward);
        GoodsManager.Instance.GetGoods(GoodsType.XP).Increase(DungeonData.BonusXpReward);
        GoodsManager.Instance.GetGoods(GoodsType.Mithril).Increase(DungeonData.MithrilRewardAmount);

        OnDungeonCleared?.Invoke();
    }

    private void HandlePlayerDied(Entity entity)
    {
        if (entity != player)
            return;

        waveController.Clear();
        TransitionTo(BattleState.PlayerDead);
    }

    private void EnterPlayerDead()
    {
        Debug.Log("[BattleManager] 플레이어 사망.");
        // UI에서 OnStateChanged(PlayerDead)를 받아 재시도/종료 화면을 표시한다.
    }

    // ─── 내부 유틸 ────────────────────────────────────────────────
    private IEnumerator DelayedTransition(BattleState next, float delay)
    {
        yield return YieldInstructionCache.WaitForSeconds(delay);
        TransitionTo(next);
    }
}
