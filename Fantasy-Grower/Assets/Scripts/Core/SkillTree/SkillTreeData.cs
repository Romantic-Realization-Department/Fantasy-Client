using System.Collections.Generic;
using UnityEngine;

public enum SkillTreeStrategyType { Branching, RowSelect, Linear }

/// <summary>
/// 직업별 스킬 트리 전체 정의. 에디터에서 AllNodes에 노드를 등록하고
/// StrategyType으로 해금 규칙 방식을 선택한다.
/// </summary>
[CreateAssetMenu(fileName = "SkillTreeData", menuName = "ScriptableObjects/SkillTree/Tree")]
public class SkillTreeData : ScriptableObject
{
    [Header("직업 정보")]
    public string JobClassName;

    [Header("트리 전략")]
    public SkillTreeStrategyType StrategyType;

    [Header("노드 목록")]
    public List<SkillNodeData> AllNodes;

    [Header("액티브 슬롯 제한")]
    public int MaxActiveSkillSlots = 3;

    /// <summary>
    /// StrategyType에 맞는 전략 인스턴스를 생성한다.
    /// </summary>
    public ISkillTreeStrategy CreateStrategy()
    {
        return StrategyType switch
        {
            SkillTreeStrategyType.Branching => new BranchingTreeStrategy(),
            SkillTreeStrategyType.RowSelect => new RowSelectTreeStrategy(),
            SkillTreeStrategyType.Linear    => new LinearTreeStrategy(),
            _ => new BranchingTreeStrategy(),
        };
    }
}
