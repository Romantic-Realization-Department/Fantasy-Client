using System;
using System.Collections.Generic;
using UnityEngine;

public partial class GameManager
{
    [Serializable]
    private struct JobProfileMapping
    {
        public Career job;
        public Sprite playerProfile;
    }

    [Header("직업별 플레이어 프로필")]
    [SerializeField]
    private JobProfileMapping[] jobProfiles;

    private readonly Dictionary<Career, Sprite> _profileDictionary = new();

    private void EnsureProfileDictionary()
    {
        if (_profileDictionary.Count > 0 || jobProfiles == null)
            return;

        foreach (JobProfileMapping entry in jobProfiles)
        {
            _profileDictionary[entry.job] = entry.playerProfile;
        }
    }

    /// <summary>
    /// 특정 직업의 플레이어 프로필을 반환합니다.
    /// </summary>
    public Sprite GetPlayerProfile(Career job)
    {
        EnsureProfileDictionary();
        if (_profileDictionary.TryGetValue(job, out Sprite profile))
        {
            return profile;
        }

        Debug.LogWarning($"[GameManager] {job} 직업의 플레이어 프로필이 등록되지 않았습니다.");
        return null;
    }

    /// <summary>
    /// 현재 선택된 직업의 플레이어 프로필을 반환합니다.
    /// </summary>
    public Sprite GetCurrentPlayerProfile()
    {
        return GetPlayerProfile(selectedJob);
    }
}
