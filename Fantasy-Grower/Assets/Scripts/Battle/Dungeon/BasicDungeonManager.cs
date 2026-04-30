using UnityEngine;

public interface IStage
{
    public DungeonData[] Stages { get; }
}

public class BasicDungeonManager : MonoBehaviour, IStage
{
    [SerializeField]
    private DungeonData[] _stages;
    public DungeonData[] Stages => _stages;

    private void Awake()
    {
        BattleManager.Instance.OnDungeonCleared += NextStage;
    }

    /// <summary>
    /// 다음 스테이지로 가는 로직을 구현한다.
    /// </summary>
    private void NextStage() { }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnDungeonCleared -= NextStage;
    }
}
