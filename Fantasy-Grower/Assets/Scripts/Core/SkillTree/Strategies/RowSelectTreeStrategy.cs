using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 궁수 행-택1 트리 전략 (롤토체스식).
/// 각 TierIndex(행)에서 단 하나의 노드만 선택 가능하다.
/// 이전 행이 선택되어야 다음 행 선택 가능하다.
/// </summary>
public class RowSelectTreeStrategy : ISkillTreeStrategy
{
    public bool CanUnlock(SkillNodeData node, IReadOnlyDictionary<SkillNodeData, bool> state)
    {
        // 이미 이 행에서 선택된 노드가 있으면 불가
        bool rowAlreadySelected = state.Any(kv =>
            kv.Value && kv.Key != node && kv.Key.TierIndex == node.TierIndex
        );

        if (rowAlreadySelected)
            return false;

        // 첫 번째 행(TierIndex == 0)은 선행 조건 없이 해금 가능
        if (node.TierIndex == 0)
            return true;

        // 이전 행(TierIndex - 1)에서 선택된 노드가 있어야 다음 행 선택 가능
        bool previousRowSelected = state.Any(kv =>
            kv.Value && kv.Key.TierIndex == node.TierIndex - 1
        );

        return previousRowSelected;
    }

    public void OnNodeUnlocked(
        SkillNodeData node,
        IReadOnlyDictionary<SkillNodeData, bool> state
    ) { }
}
