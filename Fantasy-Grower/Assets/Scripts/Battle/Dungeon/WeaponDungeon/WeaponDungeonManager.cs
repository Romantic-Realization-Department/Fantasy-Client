using System;
using System.Collections;
using UnityEngine;

public class WeaponDungeonManager
    : DungeonManager<WeaponDungeonData>,
        IStageDungeon,
        IDungeonRewardProvider
{
    public enum WeaponDungeonState
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
    private WeaponDungeonData _weaponDungeonData;

    [Header("Spawn Settings")]
    [SerializeField, Tooltip("Spawn start point")]
    private Transform _spawnPoint;

    [SerializeField, Tooltip("Spawn point interval")]
    private float _spawnPointInterval = 1.0f;

    private WeaponDungeonState _weaponDungeonState = WeaponDungeonState.Idle;
    private int _currentWaveIndex;

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
    }

    public override void RetryDungeon()
    {
        _waveController.Clear();
        if (_player != null)
            _player.ResetHp();
        _currentWaveIndex = 0;
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
        GiveGoodsReward(GetReward());
        RollWeaponDrops();
        RollUpgradeScrollDrop();
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

    private void RollWeaponDrops()
    {
        if (_currentDungeonData.WeaponDrops == null)
            return;

        var equipmentManager = EquipmentManager.Instance;
        if (equipmentManager == null)
            return;

        foreach (var drop in _currentDungeonData.WeaponDrops)
        {
            if (drop.Amount == 0)
                continue;

            if (UnityEngine.Random.Range(0f, 100f) <= drop.DropChance)
                equipmentManager.GetItem(drop.WeaponID, drop.Amount);
        }
    }

    private void RollUpgradeScrollDrop()
    {
        if (_currentDungeonData.UpgradeScrollDropAmount == 0)
            return;

        if (UnityEngine.Random.Range(0f, 100f) > _currentDungeonData.UpgradeScrollDropChance)
            return;

        GoodsManager
            .Instance.GetGoods(GoodsType.UpgradeScroll)
            .Increase(_currentDungeonData.UpgradeScrollDropAmount);
    }

    private static void GiveGoodsReward(DungeonClearReward reward)
    {
        GoodsManager.Instance.GetGoods(GoodsType.Gold).Increase(reward[GoodsType.Gold]);
        GoodsManager.Instance.GetGoods(GoodsType.XP).Increase(reward[GoodsType.XP]);
        GoodsManager.Instance.GetGoods(GoodsType.Mithril).Increase(reward[GoodsType.Mithril]);
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

    public DungeonClearReward GetReward() =>
        _currentDungeonData
            ? _currentDungeonData.DungeonClearReward
            : _weaponDungeonData.DungeonClearReward;
}
