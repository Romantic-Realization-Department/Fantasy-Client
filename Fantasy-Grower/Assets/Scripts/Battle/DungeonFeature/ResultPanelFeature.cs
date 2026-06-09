using UnityEngine;

public class ResultPanelFeature : MonoBehaviour
{
    [SerializeField]
    private GameObject _clearUI;

    [SerializeField]
    private GameObject _failUI;

    private DungeonManager _dungeonManager;

    private void Awake()
    {
        _dungeonManager = DungeonManager.Instance;

        if (_dungeonManager != null)
        {
            _dungeonManager.OnDungeonCleared += OnClear;
            _dungeonManager.OnDungeonFailed += OnFailed;
        }
    }

    private void OnDestroy()
    {
        if (_dungeonManager != null)
        {
            _dungeonManager.OnDungeonCleared -= OnClear;
            _dungeonManager.OnDungeonFailed -= OnFailed;
        }
    }

    private void OnClear() => _clearUI.SetActive(true);

    private void OnFailed() => _failUI.SetActive(true);
}
