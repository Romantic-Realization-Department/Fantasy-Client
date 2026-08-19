using System;
using UnityEngine;

public class TimeLimitFeature : MonoBehaviour
{
    private DungeonManager _dungeonManager;
    private ITimeLimitedDungeon _timeLimitedDungeon;
    private float _timeLimit;

    private void Awake()
    {
        _dungeonManager = DungeonManager.Instance;

        _dungeonManager.OnDungeonStarted += OnDungeonStarted;
        enabled = false;
    }

    private void OnDungeonStarted()
    {
        if (
            _dungeonManager is ITimeLimitedDungeon timeLimitedDungeon
            && timeLimitedDungeon.IsTimeLimited
        )
        {
            _timeLimitedDungeon = timeLimitedDungeon;
            _timeLimit = timeLimitedDungeon.GetTimeLimitSeconds();
            enabled = true;
            OnStartedUI?.Invoke(timeLimitedDungeon);
        }
    }

    private void Update()
    {
        _timeLimit -= Time.deltaTime;
        if (_timeLimit <= 0f)
        {
            _timeLimit = 0f;
            enabled = false;
            _timeLimitedDungeon?.OnTimeFinished();
        }
        OnUpdateUI?.Invoke(_timeLimit);
    }

    public event Action<ITimeLimitedDungeon> OnStartedUI;

    public event Action<float> OnUpdateUI;
}
