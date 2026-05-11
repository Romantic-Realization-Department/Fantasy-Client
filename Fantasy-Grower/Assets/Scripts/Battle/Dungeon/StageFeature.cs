using UnityEngine;

/// <summary>
/// 이 클래스는 존재하는 것만으로 이벤트를 구독하여 던전 클리어 시 다음 스테이지로 넘어가는 역할을 한다.
/// </summary>
public class StageFeature : MonoBehaviour
{
    [SerializeField, Tooltip("스테이지 데이터 배열")]
    private DungeonData[] _stages;

    [SerializeField, Tooltip("현재 스테이지 인덱스")]
    private int _currentStageIndex = 0;

    private void Awake()
    {
        BattleManager.Instance.OnDungeonCleared += NextStage;

        BattleManager.Instance.DungeonData = _stages[_currentStageIndex]; // 초기 스테이지 설정
        BattleManager.Instance.StartDungeon(); // 테스트용
    }

    /// <summary>
    /// 다음 스테이지로 가는 로직을 구현한다.
    /// </summary>
    private void NextStage()
    {
        BattleManager.Instance.DungeonData = GetNextStage();
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

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnDungeonCleared -= NextStage;
    }
}
