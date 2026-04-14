using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 현재 웨이브의 적 스폰과 생존 수를 추적하는 컴포넌트.
/// 모든 적이 사망하면 OnAllEnemiesDead 이벤트를 발화한다.
/// </summary>
public class WaveController : MonoBehaviour
{
    /// <summary>현재 웨이브의 모든 적이 사망했을 때 발화된다.</summary>
    public static event Action OnAllEnemiesDead;

    private int _aliveCount;
    private readonly HashSet<Enemy> _activeEnemies = new();

    private void OnDestroy()
    {
        foreach (Enemy e in _activeEnemies)
        {
            if (e != null)
                e.OnDied -= OnEnemyDied;
        }
    }

    /// <summary>
    /// 웨이브 데이터에 따라 적을 스폰한다.
    /// 스폰된 Enemy 인스턴스 목록을 반환한다.
    /// </summary>
    public IReadOnlyCollection<Enemy> SpawnWave(WaveData waveData, Transform[] spawnPoints)
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[WaveController] 스폰 포인트가 설정되지 않았습니다.");
            return null;
        }

        _activeEnemies.Clear();
        int spawnIndex = 0;

        // 추후 오브젝트 풀링 작동 방식으로 변환할 것.
        foreach (var entry in waveData.Entries)
        {
            for (int i = 0; i < entry.Count; i++)
            {
                Vector3 pos = spawnPoints[spawnIndex % spawnPoints.Length].position;
                GameObject go = Instantiate(entry.EnemyPrefab, pos, Quaternion.identity);

                if (!go.TryGetComponent<Enemy>(out Enemy enemy))
                {
                    Debug.LogWarning(
                        $"[WaveController] 프리팹 {entry.EnemyPrefab.name}에 Enemy 컴포넌트가 없습니다."
                    );
                    Destroy(go);
                    continue;
                }

                enemy.OnDied += OnEnemyDied;
                _activeEnemies.Add(enemy);
                spawnIndex++;
            }
        }

        _aliveCount = _activeEnemies.Count;

        if (_aliveCount == 0)
        {
            Debug.LogWarning("[WaveController] 웨이브에 적이 없습니다. 즉시 완료 처리됩니다.");
            OnAllEnemiesDead?.Invoke();
        }

        return _activeEnemies;
    }

    /// <summary>현재 살아있는 첫 번째 적을 반환한다. 없으면 null.</summary>
    public Enemy GetFirstAliveEnemy() => _activeEnemies.FirstOrDefault(e => e != null && e.Hp > 0);

    public IReadOnlyCollection<Enemy> ActiveEnemies => _activeEnemies;

    /// <summary>살아있는 적을 모두 파괴하고 상태를 초기화한다. 재시도 또는 씬 정리 시 사용.</summary>
    public void Clear()
    {
        foreach (Enemy e in _activeEnemies)
        {
            if (e != null)
            {
                e.OnDied -= OnEnemyDied;
                Destroy(e.gameObject);
            }
        }
        _activeEnemies.Clear();
        _aliveCount = 0;
    }

    private void OnEnemyDied(Entity entity)
    {
        if (entity is not Enemy enemy || !_activeEnemies.Contains(enemy))
            return;

        entity.OnDied -= OnEnemyDied;
        _aliveCount = Mathf.Max(0, _aliveCount - 1);

        if (_aliveCount == 0)
            OnAllEnemiesDead?.Invoke();
    }
}
