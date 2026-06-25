using UnityEngine;

/// <summary>
/// 에디터 타임의 하드코딩된 Player 참조를 대체하고,
/// 선택된 직업에 맞는 플레이어 프리팹을 동적으로 스폰하여 주입(Inject)하는 Feature 클래스.
/// </summary>
public class PlayerInjectionFeature : MonoBehaviour
{
    [SerializeField, Tooltip("플레이어가 생성될 위치")]
    private Transform _playerSpawnPoint;

    private void Start()
    {
        // 1. GameManager에서 현재 직업 프리팹 가져오기
        var gameManager = GameManager.InstanceOrNull;
        if (gameManager == null)
        {
            Debug.LogError(
                "[PlayerInjectionFeature] GameManager가 존재하지 않습니다. 단독 씬 테스트인 경우 확인 요망."
            );
            return;
        }

        GameObject prefab = gameManager.GetCurrentPlayerPrefab();
        if (prefab == null)
        {
            Debug.LogError("[PlayerInjectionFeature] 선택된 직업의 프리팹을 가져올 수 없습니다.");
            return;
        }

        // 2. 프리팹 생성
        Vector3 spawnPos = _playerSpawnPoint != null ? _playerSpawnPoint.position : Vector3.zero;
        Quaternion spawnRot =
            _playerSpawnPoint != null ? _playerSpawnPoint.rotation : Quaternion.identity;

        GameObject playerObj = Instantiate(prefab, spawnPos, spawnRot);

        if (!playerObj.TryGetComponent<Player>(out var player))
        {
            Debug.LogError(
                "[PlayerInjectionFeature] 생성된 프리팹에 Player 컴포넌트가 존재하지 않습니다."
            );
            return;
        }

        // 3. DungeonManager에 의존성 주입
        if (DungeonManager.Instance is IPlayerInjectable injectable)
        {
            injectable.InjectPlayer(player);
            Debug.Log($"[PlayerInjectionFeature] {playerObj.name} 주입 성공!");
        }
        else
        {
            Debug.LogWarning(
                "[PlayerInjectionFeature] 현재 던전 매니저가 IPlayerInjectable 인터페이스를 상속하지 않았습니다."
            );
        }
    }

    private void OnValidate()
    {
        if (_playerSpawnPoint == null)
        {
            Debug.LogError(
                "[PlayerInjectionFeature] Player Spawn Point가 지정되지 않았습니다.",
                this
            );
        }
    }
}
