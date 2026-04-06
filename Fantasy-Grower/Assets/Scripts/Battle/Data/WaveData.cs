using System.Linq;
using UnityEngine;

/// <summary>
/// 던전의 단일 웨이브 구성 데이터.
/// 적 프리팹과 수량을 지정하여 에디터에서 웨이브를 설계한다.
/// </summary>
[CreateAssetMenu(fileName = "WaveData", menuName = "Battle/WaveData")]
public class WaveData : ScriptableObject
{
    [System.Serializable]
    public struct EnemySpawnEntry
    {
        public GameObject enemyPrefab;
        public int count;
    }

    [Header("스폰 목록 (적 종류 × 수량)")]
    public EnemySpawnEntry[] entries;

    /// <summary>이 웨이브의 총 적 수</summary>
    public int TotalEnemyCount => entries?.Sum(e => e.count) ?? 0;
}
