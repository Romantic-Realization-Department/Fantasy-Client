using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 법사 선형 속성 트리 전략.
/// 속성(Fire/Ice/Wind) 선택 후 해당 AttributeTag 라인만 진행 가능하다.
/// 선행 노드가 모두 해금되어야 다음 노드 해금 가능하다.
/// </summary>
public class LinearTreeStrategy : ISkillTreeStrategy
{
    private string selectedAttribute;

    /// <summary>
    /// 속성을 선택한다. 첫 번째 노드를 해금할 때 자동 결정되며,
    /// 이후 다른 속성 라인은 해금 불가 상태가 된다.
    /// </summary>
    public void SelectAttribute(string attribute)
    {
        selectedAttribute = attribute;
    }

    public bool CanUnlock(SkillNodeData node, IReadOnlyDictionary<SkillNodeData, bool> state)
    {
        // 속성이 아직 선택되지 않은 경우: 어떤 속성이든 첫 번째 노드(TierIndex == 0) 해금 가능
        if (string.IsNullOrEmpty(selectedAttribute))
            return node.TierIndex == 0;

        // 선택된 속성 라인만 허용
        if (node.AttributeTag != selectedAttribute)
            return false;

        // 선행 노드가 없으면 해금 가능
        if (node.Prerequisites == null || node.Prerequisites.Length == 0)
            return true;

        // 선형 트리: 모든 선행 노드가 해금되어 있어야 한다
        return node.Prerequisites.All(pre =>
            pre != null && state.TryGetValue(pre, out bool unlocked) && unlocked
        );
    }

    public void OnNodeUnlocked(SkillNodeData node, IReadOnlyDictionary<SkillNodeData, bool> state)
    {
        // 첫 번째 노드 해금 시 속성을 확정한다
        if (string.IsNullOrEmpty(selectedAttribute) && !string.IsNullOrEmpty(node.AttributeTag))
            SelectAttribute(node.AttributeTag);
    }
}
