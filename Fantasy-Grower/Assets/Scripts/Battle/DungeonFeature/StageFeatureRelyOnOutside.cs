using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// StageFeature의 기능을 외부에 의존하여 구현한 컴포넌트 입니다.
/// </summary>
public class StageFeatureRelyOnOutside : MonoBehaviour
{
    [SerializeField, Tooltip("스테이지 데이터 배열")]
    private DungeonData[] _stages;

    [SerializeField, Tooltip("현재 스테이지 인덱스")]
    private int _currentStageIndex = 0;

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

    /// <summary>
    /// 다음 스테이지로 가는 로직을 구현한다.
    /// </summary>
    public void NextStage()
    {
        _stageDungeon.SetStage(GetNextStage());
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
