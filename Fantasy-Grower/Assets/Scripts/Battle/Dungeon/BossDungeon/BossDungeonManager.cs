using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDungeonManager
    : DungeonManager<BossDungeonData>,
        IStageDungeon,
        IDungeonRewardRecorder
{
    public enum BossDungeonState
    {
        Idle,
        WaveStart,
        Fighting,
        WaveCleared,
    }

    [SerializeField, Tooltip("Player character")]
    private Player _player;

    [SerializeField, Tooltip("Wave controller")]
    private WaveController _waveController;

    [SerializeField, Tooltip("Dungeon data")]
    private BossDungeonData _bossDungeonData;

    [Header("Spawn Settings")]
    [SerializeField, Tooltip("Spawn start point")]
    private Transform _spawnPoint;

    [SerializeField, Tooltip("Spawn point interval")]
    private float _spawnPointInterval = 1.0f;

    private BossDungeonState _bossDungeonState = BossDungeonState.Idle;
    private int _currentWaveIndex;

    public event Action<BossDungeonState> OnBossDungeonStateChanged;
    public event Action<int> OnWaveChanged;

    protected override void Init()
    {
        WaveController.OnAllEnemiesDead += HandleAllEnemiesDead;
        if (_player != null)
            _player.OnDied += HandlePlayerDied;
        SceneChanger.SceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded()
    {
        StartDungeon(_bossDungeonData);
    }

    protected override void Clear()
    {
        WaveController.OnAllEnemiesDead -= HandleAllEnemiesDead;
        if (_player != null)
            _player.OnDied -= HandlePlayerDied;
        SceneChanger.SceneLoaded -= OnSceneLoaded;
    }

    protected override void StartDungeonInternal(BossDungeonData dungeonData)
    {
        _currentWaveIndex = 0;
        _gottenWeapon = null;
    }

    public override void RetryDungeon()
    {
        _waveController.Clear();
        if (_player != null)
            _player.ResetHp();
        _currentWaveIndex = 0;
        _gottenWeapon = null;
        base.RetryDungeon();
    }

    private void TransitionTo(BossDungeonState newState)
    {
        if (_delayedTransitionCoroutine != null)
        {
            StopCoroutine(_delayedTransitionCoroutine);
            _delayedTransitionCoroutine = null;
        }

        _bossDungeonState = newState;
        OnBossDungeonStateChanged?.Invoke(_bossDungeonState);
        Debug.Log($"[BossDungeonManager] State changed: {newState}");

        switch (_bossDungeonState)
        {
            case BossDungeonState.WaveStart:
                OnWaveStart();
                break;
            case BossDungeonState.Fighting:
                break;
            case BossDungeonState.WaveCleared:
                OnWaveCleared();
                break;
        }
    }

    private void OnWaveStart()
    {
        if (_currentDungeonData.Waves == null || _currentDungeonData.Waves.Length == 0)
        {
            Debug.LogWarning("[BossDungeonManager] Wave data is empty.");
            TransitionTo(DungeonState.Cleared);
            return;
        }

        Debug.Log(
            $"[BossDungeonManager] Wave {_currentWaveIndex + 1} / {_currentDungeonData.Waves.Length} started"
        );
        OnWaveChanged?.Invoke(_currentWaveIndex);

        WaveData wave = _currentDungeonData.Waves[_currentWaveIndex];
        _waveController.SpawnWave(wave, _spawnPoint, _spawnPointInterval);

        TransitionTo(BossDungeonState.Fighting);
    }

    private void HandleAllEnemiesDead()
    {
        if (!IsRunning)
            return;

        TransitionTo(BossDungeonState.WaveCleared);
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
                DelayedTransition(BossDungeonState.WaveStart, _currentDungeonData.NextWaveDelay)
            );
        }
    }

    protected override void OnPrepareDungeon()
    {
        TransitionTo(DungeonState.Running);
    }

    protected override void OnRunningDungeon()
    {
        TransitionTo(BossDungeonState.WaveStart);
    }

    protected override void OnClearDungeon()
    {
        GoodsManager goodsManager = GoodsManager.Instance;
        DungeonClearReward dungeonClearReward = _currentDungeonData.DungeonClearReward;

        goodsManager.GetGoods(GoodsType.Mithril).Increase(dungeonClearReward[GoodsType.Mithril]);
        goodsManager.GetGoods(GoodsType.XP).Increase(dungeonClearReward[GoodsType.XP]);

        GiveRandomAGradeWeapon();
        Debug.Log("[BossDungeonManager] Dungeon cleared.");
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
        Debug.Log("[BossDungeonManager] Player died.");
    }

    private IEnumerator DelayedTransition(BossDungeonState next, float delay)
    {
        yield return YieldInstructionCache.WaitForSeconds(delay);
        TransitionTo(next);
    }

    private KeyValuePair<WeaponID, uint>? _gottenWeapon;

    private void GiveRandomAGradeWeapon()
    {
        if (
            _currentDungeonData.AGradeWeaponCandidates == null
            || _currentDungeonData.AGradeWeaponCandidates.Length == 0
            || _currentDungeonData.WeaponRewardAmount == 0
        )
        {
            return;
        }

        var equipmentManager = EquipmentManager.Instance;
        if (equipmentManager == null)
            return;

        int index = UnityEngine.Random.Range(0, _currentDungeonData.AGradeWeaponCandidates.Length);
        equipmentManager.GetItem(
            _currentDungeonData.AGradeWeaponCandidates[index],
            _currentDungeonData.WeaponRewardAmount
        );
        _gottenWeapon = new(
            _currentDungeonData.AGradeWeaponCandidates[index],
            _currentDungeonData.WeaponRewardAmount
        );
    }

    public void SetStage(DungeonData dungeonData)
    {
        if (dungeonData == null)
        {
            Debug.LogError("[BossDungeonManager] Dungeon data is empty.");
            return;
        }

        if (dungeonData is BossDungeonData bossData)
        {
            StartDungeon(bossData);
        }
        else
        {
            Debug.LogError(
                $"[BossDungeonManager] Invalid dungeon data type: {dungeonData.GetType().Name}"
            );
        }
    }

    private readonly List<RewardDisplayItem> _rewardDisplayItems = new();

    public IReadOnlyList<RewardDisplayItem> GetRewardItems()
    {
        _rewardDisplayItems.Clear();

        _rewardDisplayItems.Add(
            RewardDisplayItemFactory.Goods(
                GoodsType.Mithril,
                _currentDungeonData.DungeonClearReward[GoodsType.Mithril]
            )
        );
        _rewardDisplayItems.Add(
            RewardDisplayItemFactory.Goods(
                GoodsType.XP,
                _currentDungeonData.DungeonClearReward[GoodsType.XP]
            )
        );

        if (_gottenWeapon.HasValue)
        {
            var weapon = _gottenWeapon.Value;
            _rewardDisplayItems.Add(RewardDisplayItemFactory.Weapon(weapon.Key, weapon.Value));
        }

        return _rewardDisplayItems;
    }
}
