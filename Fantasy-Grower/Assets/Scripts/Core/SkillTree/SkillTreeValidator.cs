using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 스킬 트리 노드 해금의 공통 조건을 검증하는 정적 유틸리티.
/// 전략별 규칙(ISkillTreeStrategy)과 독립적으로 동작한다.
/// </summary>
public static class SkillTreeValidator
{
    /// <summary>
    /// 선행 노드가 모두 해금되어 있는지 확인한다.
    /// </summary>
    public static bool ArePrerequisitesMet(
        SkillNodeData node,
        IReadOnlyDictionary<SkillNodeData, bool> state)
    {
        if (node.Prerequisites == null || node.Prerequisites.Length == 0)
            return true;

        return node.Prerequisites
            .All(pre => pre != null && state.TryGetValue(pre, out bool unlocked) && unlocked);
    }

    /// <summary>
    /// SP가 해금 비용을 충당할 수 있는지 확인한다.
    /// </summary>
    public static bool HasEnoughSP(SkillNodeData node, SO_SP sp)
    {
        if (node.Skill == null) return false;
        return sp.Get() >= (uint)node.Skill.SPCost;
    }

    /// <summary>
    /// 액티브 스킬 슬롯에 여유가 있는지 확인한다.
    /// </summary>
    public static bool HasActiveSlotAvailable(
        IReadOnlyList<ActiveSkillData> equipped,
        int maxSlots)
    {
        int occupiedSlots = equipped.Count(skill => skill != null);
        return occupiedSlots < maxSlots;
    }
}
