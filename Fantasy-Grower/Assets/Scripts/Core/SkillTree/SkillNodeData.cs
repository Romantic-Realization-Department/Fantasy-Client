using UnityEngine;

/// <summary>
/// 스킬 트리의 단일 노드. 에디터에서 드래그&드롭으로 선행 노드를 연결한다.
/// </summary>
[CreateAssetMenu(fileName = "SkillNode", menuName = "ScriptableObjects/SkillTree/Node")]
public class SkillNodeData : ScriptableObject
{
    [Header("스킬 참조")]
    public SkillData Skill;

    [Header("트리 구조")]
    public SkillNodeData[] Prerequisites;
    public int TierIndex; // 0 = 기본 평타, 1 = 1티어, 2 = 2티어, ...
    public int SlotIndex; // 같은 티어 내 위치

    [Header("속성 태그 (법사 전용)")]
    public string AttributeTag; // "Fire" / "Ice" / "Wind"
}
