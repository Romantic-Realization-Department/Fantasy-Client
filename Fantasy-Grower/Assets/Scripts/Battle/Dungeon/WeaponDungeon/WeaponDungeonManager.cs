using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponDungeonManager
    : DungeonManager<WeaponDungeonData>,
        IStageDungeon,
        IDungeonRewardRecorder
{
    public enum WeaponDungeonState
    {
        Idle,
        WaveStart,
        Fighting,
        WaveCleared,
    }

    [SerializeField]
    private Player _player;

    [SerializeField]
    private WaveController _waveController;

    [SerializeField, Tooltip("던전 데이터")]
    private WeaponDungeonData _weaponDungeonData;

    [Header("Spawn Settings")]
    [SerializeField, Tooltip("적이 스폰될 지점")]
    private Transform _spawnPoint;

    [SerializeField, Tooltip("적의 스폰 간격")]
    private float _spawnPointInterval = 1.0f;

    // 런타임 데이터
    private WeaponDungeonState _weaponDungeonState = WeaponDungeonState.Idle;
    private int _currentWaveIndex;
    private uint _upgradeScrollInitialValue;
    private uint _earnedUpgradeScrollAmount;

    // 이벤트
    public event Action<WeaponDungeonState> OnWeaponDungeonStateChanged;
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
        StartDungeon(_weaponDungeonData);
    }

    protected override void Clear()
    {
        WaveController.OnAllEnemiesDead -= HandleAllEnemiesDead;
        if (_player != null)
            _player.OnDied -= HandlePlayerDied;
        SceneChanger.SceneLoaded -= OnSceneLoaded;
    }

    protected override void StartDungeonInternal(WeaponDungeonData dungeonData)
    {
        _currentWaveIndex = 0;
        _upgradeScrollInitialValue = GetUpgradeScrollValue();
        _earnedUpgradeScrollAmount = 0;
        _gottenWeapons.Clear();
    }

    public override void RetryDungeon()
    {
        _waveController.Clear();
        if (_player != null)
            _player.ResetHp();
        _currentWaveIndex = 0;
        _upgradeScrollInitialValue = GetUpgradeScrollValue();
        _earnedUpgradeScrollAmount = 0;
        _gottenWeapons.Clear();
        base.RetryDungeon();
    }

    private void TransitionTo(WeaponDungeonState newState)
    {
        if (_delayedTransitionCoroutine != null)
        {
            StopCoroutine(_delayedTransitionCoroutine);
            _delayedTransitionCoroutine = null;
        }

        _weaponDungeonState = newState;
        OnWeaponDungeonStateChanged?.Invoke(_weaponDungeonState);
        Debug.Log($"[WeaponDungeonManager] State changed: {newState}");

        switch (_weaponDungeonState)
        {
            case WeaponDungeonState.WaveStart:
                OnWaveStart();
                break;
            case WeaponDungeonState.Fighting:
                break;
            case WeaponDungeonState.WaveCleared:
                OnWaveCleared();
                break;
        }
    }

    private void OnWaveStart()
    {
        if (_currentDungeonData.Waves == null || _currentDungeonData.Waves.Length == 0)
        {
            Debug.LogWarning("[WeaponDungeonManager] Wave data is empty.");
            TransitionTo(DungeonState.Cleared);
            return;
        }

        Debug.Log(
            $"[WeaponDungeonManager] Wave {_currentWaveIndex + 1} / {_currentDungeonData.Waves.Length} started"
        );
        OnWaveChanged?.Invoke(_currentWaveIndex);

        WaveData wave = _currentDungeonData.Waves[_currentWaveIndex];
        _waveController.SpawnWave(wave, _spawnPoint, _spawnPointInterval);

        TransitionTo(WeaponDungeonState.Fighting);
    }

    private void HandleAllEnemiesDead()
    {
        if (!IsRunning)
            return;

        TransitionTo(WeaponDungeonState.WaveCleared);
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
                DelayedTransition(WeaponDungeonState.WaveStart, _currentDungeonData.NextWaveDelay)
            );
        }
    }

    protected override void OnPrepareDungeon()
    {
        TransitionTo(DungeonState.Running);
    }

    protected override void OnRunningDungeon()
    {
        TransitionTo(WeaponDungeonState.WaveStart);
    }

    protected override void OnClearDungeon()
    {
        RollWeaponDrops();
        RollUpgradeScrollDrop();
        uint currentUpgradeScrollValue = GetUpgradeScrollValue();
        _earnedUpgradeScrollAmount =
            currentUpgradeScrollValue >= _upgradeScrollInitialValue
                ? currentUpgradeScrollValue - _upgradeScrollInitialValue
                : 0;
        Debug.Log("[WeaponDungeonManager] Dungeon cleared.");
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
        Debug.Log("[WeaponDungeonManager] Player died.");
    }

    private IEnumerator DelayedTransition(WeaponDungeonState next, float delay)
    {
        yield return YieldInstructionCache.WaitForSeconds(delay);
        TransitionTo(next);
    }

    private readonly Dictionary<WeaponID, uint> _gottenWeapons = new();

    private void RollWeaponDrops()
    {
        _gottenWeapons.Clear();

        if (_currentDungeonData == null || _currentDungeonData.WeaponDrops == null)
            return;

        var equipmentManager = EquipmentManager.Instance;
        if (equipmentManager == null)
            return;

        foreach (var drop in _currentDungeonData.WeaponDrops)
        {
            if (drop.Amount == 0)
                continue;

            if (UnityEngine.Random.Range(0f, 100f) <= drop.DropChance)
            {
                equipmentManager.GetItem(drop.WeaponID, drop.Amount);
                if (_gottenWeapons.TryGetValue(drop.WeaponID, out uint value))
                {
                    _gottenWeapons[drop.WeaponID] = value + drop.Amount;
                }
                else
                {
                    _gottenWeapons[drop.WeaponID] = drop.Amount;
                }
            }
        }
    }

    private void RollUpgradeScrollDrop()
    {
        if (_currentDungeonData.UpgradeScrollDropAmount == 0)
            return;

        if (UnityEngine.Random.Range(0f, 100f) > _currentDungeonData.UpgradeScrollDropChance)
            return;

        SO_Goods upgradeScroll = GoodsManager.Instance.GetGoods(GoodsType.UpgradeScroll);

        if (upgradeScroll != null)
            upgradeScroll.Increase(_currentDungeonData.UpgradeScrollDropAmount);
    }

    private static uint GetUpgradeScrollValue()
    {
        var upgradeScroll = GoodsManager.Instance.GetGoods(GoodsType.UpgradeScroll);
        return upgradeScroll != null ? upgradeScroll.Get() : 0;
    }

    public void SetStage(DungeonData dungeonData)
    {
        if (dungeonData == null)
        {
            Debug.LogError("[WeaponDungeonManager] Dungeon data is empty.");
            return;
        }

        if (dungeonData is WeaponDungeonData weaponData)
        {
            StartDungeon(weaponData);
        }
        else
        {
            Debug.LogError(
                $"[WeaponDungeonManager] Invalid dungeon data type: {dungeonData.GetType().Name}"
            );
        }
    }

    private readonly List<RewardDisplayItem> _rewards = new();

    public IReadOnlyList<RewardDisplayItem> GetRewardItems()
    {
        _rewards.Clear();

        foreach (var weapon in _gottenWeapons)
        {
            _rewards.Add(RewardDisplayItemFactory.Weapon(weapon.Key, weapon.Value));
        }

        _rewards.Add(
            RewardDisplayItemFactory.Goods(GoodsType.UpgradeScroll, _earnedUpgradeScrollAmount)
        );

        return _rewards;
    }
}
