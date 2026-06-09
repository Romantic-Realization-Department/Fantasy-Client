using System;
using System.Collections;
using UnityEngine;

public abstract class DungeonManager<DataT> : DungeonManager
    where DataT : DungeonData
{
    // 현재 던전 데이터
    protected DataT _currentDungeonData;

    // 던전 시작 로직 구현 (예: 웨이브 초기화, UI 갱신 등)
    public void StartDungeon(DataT dungeonData)
    {
        if (dungeonData == null)
        {
            Debug.LogError("[DungeonManager] DungeonData가 들어오지 않았습니다.");
            return;
        }

        _currentDungeonData = dungeonData;
        StartDungeonInternal(dungeonData);
        TransitionTo(DungeonState.Preparing);
    }

    /// <summary>
    /// 자식에서의 던전 시작 로직 구현 (예: 던전 요소 초기화, UI 갱신 등)
    /// </summary>
    /// <param name="dungeonData"></param>
    protected virtual void StartDungeonInternal(DataT dungeonData) { }
}

/// <summary>
/// DungeonManager는 Scene에 단 하나만 존재해야 합니다.
/// </summary>
public abstract class DungeonManager : MonoBehaviour
{
    private static DungeonManager _instance;
    public static DungeonManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<DungeonManager>();
                if (_instance == null)
                {
                    Debug.LogError("[DungeonManager] 씬에 DungeonManager가 존재하지 않습니다.");
                }
            }
            return _instance;
        }
    }

    public enum DungeonState
    {
        Idle,
        Preparing,
        Running,
        Cleared,
        Failed,
    }

    // ─── UI 알림 이벤트 ───────────────────────────────────────────
    /// <summary>상태가 변경될 때마다 발화된다. UI 패널 전환에 사용한다.</summary>
    public event Action<DungeonState> OnStateChanged;

    /// <summary>던전이 시작되었을 때 발화된다.</summary>
    public event Action OnDungeonStarted;

    /// <summary>던전이 클리어되었을 때 발화된다.</summary>
    public event Action OnDungeonCleared;

    /// <summary>던전에서 패배하였을 때 발화된다.</summary>
    public event Action OnDungeonFailed;

    // ─── 런타임 상태 ─────────────────────────────────────────────
    private DungeonState _state = DungeonState.Idle;
    protected Coroutine _delayedTransitionCoroutine;

    public DungeonState State => _state;
    public bool IsRunning => _state == DungeonState.Running;
    public bool IsFinished => _state == DungeonState.Cleared || _state == DungeonState.Failed;

    // ─── 유니티 라이프사이클 ──────────────────────────────────────
    protected void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogError("[DungeonManager] 씬에 DungeonManager가 2개 이상 존재합니다.");
            Destroy(this);
            return;
        }
        _instance = this;

        Init();
    }

    protected virtual void Init() { }

    protected void OnDestroy()
    {
        if (_instance == this)
        {
            Clear();
            _instance = null;
        }
    }

    protected virtual void Clear() { }

    // 던전 재시작 로직 구현 (예: 플레이어 상태 초기화, 웨이브 재설정 등)
    public virtual void RetryDungeon()
    {
        TransitionTo(DungeonState.Preparing);
    }

    // ─── 상태 머신 ────────────────────────────────────────────────
    protected void TransitionTo(DungeonState newState)
    {
        if (IsFinished && (newState == DungeonState.Cleared || newState == DungeonState.Failed))
            return;

        if (_delayedTransitionCoroutine != null)
        {
            StopCoroutine(_delayedTransitionCoroutine);
            _delayedTransitionCoroutine = null;
        }

        _state = newState;
        OnStateChanged?.Invoke(_state);
        Debug.Log($"[DungeonManager] 상태 전환: {newState}");

        switch (_state)
        {
            case DungeonState.Preparing:
                OnDungeonStarted?.Invoke();
                OnPrepareDungeon();
                break;
            case DungeonState.Running:
                OnRunningDungeon();
                break;
            case DungeonState.Cleared:
                OnClearDungeon();
                OnDungeonCleared?.Invoke();
                break;
            case DungeonState.Failed:
                OnFailDungeon();
                OnDungeonFailed?.Invoke();
                break;
        }
    }

    protected virtual void OnPrepareDungeon() { }

    protected virtual void OnRunningDungeon() { }

    protected virtual void OnClearDungeon() { }

    protected virtual void OnFailDungeon() { }

    // ─── 내부 유틸 ────────────────────────────────────────────────
    protected IEnumerator DelayedTransition(DungeonState next, float delay)
    {
        yield return YieldInstructionCache.WaitForSeconds(delay);
        TransitionTo(next);
    }

#if UNITY_EDITOR
    protected void Reset()
    {
        if (FindObjectsByType<DungeonManager>(FindObjectsSortMode.None).Length > 1)
        {
            Debug.LogError("[DungeonManager] 씬에 DungeonManager가 여러 개 존재합니다.", this);
        }
    }
#endif
}
