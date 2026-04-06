using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 플레이어의 스킬 트리 런타임 상태를 관리하는 컴포넌트.
/// 해금 상태를 Dictionary로 보관하여 ScriptableObject 에셋 오염을 방지한다.
/// </summary>
[RequireComponent(typeof(Player))]
public class SkillTreeComponent : MonoBehaviour
{
    [SerializeField] private SkillTreeData _treeData;
    [SerializeField] private SO_SP _spResource;

    // ScriptableObject를 직접 수정하지 않고 런타임 상태를 Dictionary로 격리
    private Dictionary<SkillNodeData, bool> _unlockedState;

    // 장착된 액티브 스킬 슬롯 (null = 비어있음)
    private List<ActiveSkillData> _equippedActives;

    private ISkillTreeStrategy _strategy;
    private Entity _entity;

    private void Awake()
    {
        _entity = GetComponent<Entity>();
        _unlockedState = new Dictionary<SkillNodeData, bool>();
        _strategy = _treeData != null ? _treeData.CreateStrategy() : null;

        int slotCount = _treeData != null ? _treeData.MaxActiveSkillSlots : 3;
        _equippedActives = new List<ActiveSkillData>(new ActiveSkillData[slotCount]);

        if (_treeData == null)
            Debug.LogWarning("[SkillTreeComponent] SkillTreeData가 비어 있습니다!");

        if (_spResource == null)
            Debug.LogWarning("[SkillTreeComponent] SO_SP가 비어 있습니다!");

        // 모든 노드를 초기 잠금 상태로 등록
        if (_treeData != null && _treeData.AllNodes != null)
        {
            foreach (var node in _treeData.AllNodes)
                _unlockedState[node] = false;
        }
    }

    // ─── 해금 흐름 ───────────────────────────────────────────────

    /// <summary>
    /// UI에서 노드 선택 시 진입점. SP 소비 + 전략 조건 모두 통과해야 해금된다.
    /// </summary>
    public bool TryUnlockNode(SkillNodeData node)
    {
        if (node == null || _strategy == null) return false;

        if (IsUnlocked(node))
        {
            Debug.Log($"[SkillTree] {node.Skill?.SkillName}은 이미 해금되어 있습니다.");
            return false;
        }

        if (!CanUnlock(node))
        {
            Debug.Log($"[SkillTree] {node.Skill?.SkillName} 해금 조건 미충족.");
            return false;
        }

        // SP 소비
        _spResource.Decrease((uint)node.Skill.SPCost);

        _unlockedState[node] = true;
        _strategy.OnNodeUnlocked(node, _unlockedState);

        Debug.Log($"[SkillTree] {node.Skill?.SkillName} 해금 완료.");

        RecalculatePassives();
        return true;
    }

    /// <summary>
    /// 해금된 모든 패시브 스킬 효과를 재계산하여 Entity 스탯에 반영한다.
    /// </summary>
    private void RecalculatePassives()
    {
        var modifier = EntityStatModifier.Zero;

        foreach (var kv in _unlockedState)
        {
            if (!kv.Value) continue;
            if (kv.Key.Skill is PassiveSkillData passive)
                passive.ApplyPassive(ref modifier);
        }

        _entity.ApplyStatModifier(modifier);
    }

    // ─── 액티브 스킬 장착 ─────────────────────────────────────────

    /// <summary>
    /// 해금된 액티브 스킬을 지정 슬롯에 장착한다.
    /// </summary>
    public bool TryEquipActiveSkill(ActiveSkillData skill, int slotIndex)
    {
        if (skill == null || slotIndex < 0 || slotIndex >= _equippedActives.Count)
            return false;

        if (!IsUnlocked(FindNodeBySkill(skill)))
        {
            Debug.Log($"[SkillTree] {skill.SkillName}이 해금되지 않았습니다.");
            return false;
        }

        _equippedActives[slotIndex] = skill;
        return true;
    }

    public void UnequipActiveSkill(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < _equippedActives.Count)
            _equippedActives[slotIndex] = null;
    }

    public ActiveSkillData GetEquippedSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _equippedActives.Count) return null;
        return _equippedActives[slotIndex];
    }

    // ─── 조회 ────────────────────────────────────────────────────

    public bool IsUnlocked(SkillNodeData node)
    {
        return node != null && _unlockedState.TryGetValue(node, out bool v) && v;
    }

    /// <summary>
    /// SP 조건과 전략 조건을 통합하여 해금 가능 여부를 반환한다.
    /// </summary>
    public bool CanUnlock(SkillNodeData node)
    {
        if (node == null || node.Skill == null) return false;
        if (!SkillTreeValidator.HasEnoughSP(node, _spResource)) return false;
        return _strategy.CanUnlock(node, _unlockedState);
    }

    public IReadOnlyList<ActiveSkillData> GetEquippedActives() => _equippedActives.AsReadOnly();

    // ─── 내부 유틸 ────────────────────────────────────────────────

    private SkillNodeData FindNodeBySkill(SkillData skill)
    {
        return _treeData?.AllNodes?.FirstOrDefault(n => n.Skill == skill);
    }
}
