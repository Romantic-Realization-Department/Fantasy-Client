using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// StageFeature의 기능을 외부에 의존하여 구현한 컴포넌트 입니다.
/// </summary>
public class StageFeatureRelyOnOutside : MonoBehaviour, IStageProvider
{
    [SerializeField, Tooltip("스테이지 데이터 배열")]
    private DungeonData[] _stages;

    [SerializeField, Tooltip("현재 스테이지 인덱스")]
    private int _currentStageIndex = 0;

    public int CurrentStageIndex => _currentStageIndex;
    public event Action<int> OnStageChanged;

    private IStageDungeon _stageDungeon;

    private void Awake()
    {
        if (DungeonManager.Instance is IStageDungeon stageDungeon)
            _stageDungeon = stageDungeon;
        else
        {
            Debug.LogError(
                "DungeonManager가 IStageDungeon 인터페이스를 구현하지 않았습니다. StageFeature가 정상적으로 작동하지 않을 수 있습니다."
            );
            return;
        }
    }

    private void Start()
    {
        // 최초 UI 갱신을 위해 이벤트 발송
        if (_stages != null && _stages.Length > 0)
        {
            OnStageChanged?.Invoke(_currentStageIndex);
        }
    }

    /// <summary>
    /// 다음 스테이지로 가는 로직을 구현한다.
    /// </summary>
    public void NextStage()
    {
        DungeonData nextStageData = GetNextStage();

        // 현재 도달한 스테이지 기록 갱신 (인덱스는 0부터 시작하므로 +1)
        if (
            _stages != null
            && _stages.Length > 0
            && _currentStageIndex >= 0
            && _currentStageIndex < _stages.Length
        )
        {
            GameManager.Instance.UpdateDungeonRecord(
                _stages[_currentStageIndex].DungeonType,
                _currentStageIndex + 1
            );
        }

        if (nextStageData != null)
        {
            _stageDungeon.SetStage(nextStageData);
            OnStageChanged?.Invoke(_currentStageIndex);
        }
    }

    /// <summary>
    /// 원하는 스테이지 인덱스로 강제 이동합니다.
    /// </summary>
    /// <param name="targetIndex">이동할 스테이지의 인덱스 (0부터 시작)</param>
    public void JumpToStage(int targetIndex)
    {
        if (_stages == null || _stages.Length == 0)
            return;

        // 인덱스가 배열 범위를 벗어나지 않도록 방어 (0 ~ Length - 1)
        targetIndex = Mathf.Clamp(targetIndex, 0, _stages.Length - 1);

        // 이미 해당 스테이지라면 무시
        if (_currentStageIndex == targetIndex)
            return;

        _currentStageIndex = targetIndex;
        DungeonData targetData = _stages[_currentStageIndex];

        if (targetData != null)
        {
            _stageDungeon.SetStage(targetData);
            OnStageChanged?.Invoke(_currentStageIndex);
        }
    }

    /// <summary>
    /// 다음 스테이지를 반환하는 메서드. 다음 스테이지가 존재하면 해당 스테이지 데이터를 반환하고, 그렇지 않으면 null을 반환한다.
    /// </summary>
    /// <returns>다음 스테이지의 DungeonData 또는 null</returns>
    private DungeonData GetNextStage()
    {
        if (_currentStageIndex + 1 < _stages.Length) // 다음 스테이지가 존재하는 경우, 다음 스테이지 데이터를 반환한다.
            return _stages[++_currentStageIndex];

        if (_currentStageIndex + 1 == _stages.Length) // 마지막 스테이지인 경우, 현재 스테이지를 반환한다.
            return _stages[_currentStageIndex];

        return null; // 마지막 스테이지를 넘은 경우(오류), null을 반환한다.
    }
}
