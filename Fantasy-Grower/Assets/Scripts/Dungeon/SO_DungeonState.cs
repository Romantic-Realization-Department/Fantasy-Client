using System;
using UnityEngine;

public enum DungeonState
{
    None,
    Start,
    InProgress,
    Failed,
    Completed,
}

public class SO_DungeonState : ScriptableObject
{
    private DungeonState currentState;

    /// <summary>
    /// 상태 변경 시 이벤트를 발생시키는 프로퍼티
    /// </summary>
    public DungeonState DungeonState
    {
        get => currentState;
        set
        {
            if (currentState == value)
                return;
            currentState = value;
            OnDungeonStateChanged?.Invoke(currentState);
        }
    }

    /// <summary>
    /// 사용 예 : state.OnDungeonStateChanged += (newState) => { Debug.Log($"Dungeon state changed to: {newState}"); };
    /// </summary>
    public event Action<DungeonState> OnDungeonStateChanged;
}
