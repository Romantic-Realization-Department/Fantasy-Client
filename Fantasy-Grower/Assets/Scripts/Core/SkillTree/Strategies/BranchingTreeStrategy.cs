using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 검사 분기형 트리 전략.
/// 같은 TierIndex 내에서 SlotIndex가 동일한 그룹(분기 지점)에는 하나만 선택 가능하다.
/// 선행 노드 중 하나라도 해금되어 있으면 진행 가능하다.
/// </summary>
public class BranchingTreeStrategy : ISkillTreeStrategy
{
    public bool CanUnlock(SkillNodeData node, IReadOnlyDictionary<SkillNodeData, bool> state)
    {
        // 선행 노드가 없으면 (기본 노드) 즉시 해금 가능
        if (node.Prerequisites == null || node.Prerequisites.Length == 0)
            return true;

        // 선행 노드 중 하나라도 해금되어 있으면 진행 가능
        bool anyPrerequisiteMet = node.Prerequisites.Any(pre =>
            pre != null && state.TryGetValue(pre, out bool unlocked) && unlocked
        );

        if (!anyPrerequisiteMet)
            return false;

        // 같은 TierIndex + SlotIndex를 가진 노드(분기 그룹)에서 이미 다른 노드가 해금되었다면 불가
        bool conflictExists = state
            .Where(kv => kv.Value)
            .Select(kv => kv.Key)
            .Any(unlocked =>
                unlocked != node
                && unlocked.TierIndex == node.TierIndex
                && unlocked.SlotIndex == node.SlotIndex
            );

        return !conflictExists;
    }

    public void OnNodeUnlocked(
        SkillNodeData node,
        IReadOnlyDictionary<SkillNodeData, bool> state
    ) { }
}
