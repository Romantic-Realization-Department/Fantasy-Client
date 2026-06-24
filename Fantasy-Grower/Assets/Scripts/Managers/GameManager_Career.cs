using System;
using System.Collections.Generic;
using UnityEngine;

public partial class GameManager
{
    [Serializable]
    private struct JobPrefabMapping
    {
        public Career job;
        public GameObject playerPrefab;
    }

    [Header("직업별 플레이어 프리팹")]
    [SerializeField]
    private JobPrefabMapping[] jobPrefabs;

    private readonly Dictionary<Career, GameObject> _prefabDictionary = new();

    private void EnsurePrefabDictionary()
    {
        if (_prefabDictionary.Count > 0 || jobPrefabs == null)
            return;

        foreach (JobPrefabMapping entry in jobPrefabs)
        {
            _prefabDictionary[entry.job] = entry.playerPrefab;
        }
    }

    /// <summary>
    /// 특정 직업의 플레이어 프리팹을 반환합니다.
    /// </summary>
    public GameObject GetPlayerPrefab(Career job)
    {
        EnsurePrefabDictionary();
        if (_prefabDictionary.TryGetValue(job, out GameObject prefab))
        {
            return prefab;
        }

        Debug.LogWarning($"[GameManager] {job} 직업의 플레이어 프리팹이 등록되지 않았습니다.");
        return null;
    }

    /// <summary>
    /// 현재 선택된 직업의 플레이어 프리팹을 반환합니다.
    /// </summary>
    public GameObject GetCurrentPlayerPrefab()
    {
        return GetPlayerPrefab(selectedJob);
    }
}
