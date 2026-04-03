using System;
using UnityEngine;

public enum DungeonState
{
    None, // 초기 상태, 던전이 시작되지 않은 상태
    Start, // 던전이 시작된 상태
    InProgress, // 던전이 진행 중인 상태
    Failed, // 클리어에 실패한 상태
    Completed, // 클리어에 성공한 상태
}

[CreateAssetMenu(fileName = "DungeonState", menuName = "ScriptableObjects/DungeonState", order = 1)]
public class SO_DungeonState : ScriptableObject
{
    private DungeonState currentState;

    /// <summary>
    /// 상태 변경 시 이벤트를 발생시키는 프로퍼티
    /// </summary>
    public DungeonState CurrentState
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
