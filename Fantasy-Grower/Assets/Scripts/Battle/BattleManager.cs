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
    // ─── Inspector 연결 ──────────────────────────────────────────
    [SerializeField]
    private Player player;

    [SerializeField]
    private AutoAttackController autoAttack;

    [SerializeField]
    private WaveController waveController;

    [SerializeField]
    private Transform[] spawnPoints;

    [Header("던전 데이터")]
    [SerializeField]
    private DungeonData dungeonData;

    // ─── 런타임 상태 ─────────────────────────────────────────────
    private BattleState state = BattleState.Idle;
    private int currentWaveIndex;
    private List<Enemy> currentWaveEnemies;

    // ─── UI 알림 이벤트 ───────────────────────────────────────────
    /// <summary>상태가 변경될 때마다 발화된다. UI 패널 전환에 사용한다.</summary>
    public static event Action<BattleState> OnStateChanged;

    /// <summary>새 웨이브가 시작될 때 웨이브 번호(0-based)를 전달한다.</summary>
    public static event Action<int> OnWaveChanged;

    // ─── 유니티 라이프사이클 ──────────────────────────────────────
    private void Awake()
    {
        WaveController.OnAllEnemiesDead += HandleAllEnemiesDead;
        Entity.OnDied += HandlePlayerDied;
    }

    private void OnDestroy()
    {
        WaveController.OnAllEnemiesDead -= HandleAllEnemiesDead;
        Entity.OnDied -= HandlePlayerDied;
    }

    // ─── 공개 API (UI 버튼에서 호출) ─────────────────────────────
    /// <summary>던전을 시작한다.</summary>
    public void StartDungeon()
    {
        if (dungeonData == null)
        {
            Debug.LogError("[BattleManager] DungeonData가 연결되지 않았습니다.");
            return;
        }

        if (dungeonData.DungeonType == DungeonType.Gold)
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
            $"[BattleManager] 웨이브 {currentWaveIndex + 1} / {dungeonData.Waves.Length} 시작"
        );
        OnWaveChanged?.Invoke(currentWaveIndex);

        WaveData wave = dungeonData.Waves[currentWaveIndex];
        currentWaveEnemies = waveController.SpawnWave(wave, spawnPoints);

        foreach (Enemy e in currentWaveEnemies)
            e.GetComponent<EnemyAI>()?.Initialize(player);

        TransitionTo(BattleState.Fighting);
    }

    private void EnterFighting()
    {
        autoAttack.StartAutoAttack();

        foreach (Enemy e in currentWaveEnemies)
            e.GetComponent<EnemyAI>()?.StartAttacking();
    }

    private void HandleAllEnemiesDead()
    {
        autoAttack.StopAutoAttack();

        foreach (Enemy e in currentWaveEnemies)
            e.GetComponent<EnemyAI>()?.StopAttacking();

        TransitionTo(BattleState.WaveCleared);
    }

    private void EnterWaveCleared()
    {
        currentWaveIndex++;

        if (currentWaveIndex >= dungeonData.Waves.Length)
        {
            TransitionTo(BattleState.DungeonCleared);
        }
        else
        {
            StartCoroutine(DelayedTransition(BattleState.WaveStart, 1.5f));
        }
    }

    private void EnterDungeonCleared()
    {
        Debug.Log("[BattleManager] 던전 클리어!");

        GoodsManager.Instance.GetGoods(GoodsType.Gold).Increase(dungeonData.BonusGoldReward);
        GoodsManager.Instance.GetGoods(GoodsType.XP).Increase(dungeonData.BonusXpReward);

        if (dungeonData.DungeonType == DungeonType.Boss)
            GoodsManager
                .Instance.GetGoods(GoodsType.Mithril)
                .Increase(dungeonData.MithrilRewardAmount);
    }

    private void HandlePlayerDied(Entity entity)
    {
        if (entity != player)
            return;

        autoAttack.StopAutoAttack();
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
        yield return new WaitForSeconds(delay);
        TransitionTo(next);
    }
}
