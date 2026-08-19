using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 전역 상태(현재 선택한 캐릭터 등)를 보관하는 싱글톤 매니저.
/// 선택된 직업을 기준으로 스킬 트리 데이터를 제공하여, 스킬 트리 구성이
/// 플레이어에 맞게 동적으로 바뀌도록 한다.
/// </summary>
public partial class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    /// <summary>
    /// 싱글톤 인스턴스. 씬에 존재하지 않으면 에러 로그를 남기고 null을 반환한다.
    /// </summary>
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<GameManager>();
                if (_instance == null)
                    Debug.LogError("GameManager instance not found in the scene!");
            }
            return _instance;
        }
    }

    /// <summary>
    /// 에러 로그 없이 인스턴스를 조회한다. GameManager 없이 동작 가능한
    /// 테스트 씬(스킬 UI 테스트 등)에서 선택적 참조용으로 사용한다.
    /// </summary>
    public static GameManager InstanceOrNull =>
        _instance != null ? _instance : (_instance = FindAnyObjectByType<GameManager>());

    [Serializable]
    private struct JobSkillTree
    {
        public Career job;
        public SkillTreeData treeData;
    }

    [Header("현재 선택된 직업")]
    [SerializeField]
    private Career selectedJob = Career.Warrior;

    [Header("직업별 스킬 트리")]
    [SerializeField]
    private JobSkillTree[] jobSkillTrees; // 인스펙터에서 직업과 스킬 트리를 매핑

    private readonly Dictionary<Career, SkillTreeData> _treeDictionary = new();

    /// <summary>선택된 직업이 변경되면 호출된다. (UI/스킬 트리 갱신용)</summary>
    public event Action<Career> OnSelectedJobChanged;

    /// <summary>현재 선택된 직업.</summary>
    public Career SelectedJob => selectedJob;

    /// <summary>현재 선택된 직업의 스킬 트리 데이터.</summary>
    public SkillTreeData CurrentSkillTreeData => GetSkillTreeData(selectedJob);

    private void Awake()
    {
        // 싱글톤 패턴: 이미 인스턴스가 존재하면 자신을 파괴
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject); // 씬이 변경되어도 유지

        _instance = this;

        EnsureDictionary();
    }

    /// <summary>
    /// 직업→스킬 트리 딕셔너리를 (필요 시) 구성한다.
    /// 스크립트 실행 순서로 Awake가 늦게 호출돼도 조회가 동작하도록 지연 구성한다.
    /// </summary>
    private void EnsureDictionary()
    {
        if (_treeDictionary.Count > 0 || jobSkillTrees == null)
            return;

        foreach (JobSkillTree entry in jobSkillTrees)
            _treeDictionary[entry.job] = entry.treeData;
    }

    /// <summary>
    /// 플레이어 선택창 등에서 직업을 선택할 때 호출한다.
    /// 값이 실제로 바뀐 경우에만 변경 이벤트를 발생시킨다.
    /// </summary>
    public void SelectJob(Career job)
    {
        if (selectedJob == job)
            return;

        selectedJob = job;
        OnSelectedJobChanged?.Invoke(selectedJob);
    }

    /// <summary>
    /// 직업에 매핑된 스킬 트리 데이터를 반환한다. (없으면 null)
    /// </summary>
    public SkillTreeData GetSkillTreeData(Career job)
    {
        EnsureDictionary();
        if (_treeDictionary.TryGetValue(job, out SkillTreeData treeData))
            return treeData;

        Debug.LogWarning($"[GameManager] {job} 직업의 스킬 트리가 등록되지 않았습니다.");
        return null;
    }
}
