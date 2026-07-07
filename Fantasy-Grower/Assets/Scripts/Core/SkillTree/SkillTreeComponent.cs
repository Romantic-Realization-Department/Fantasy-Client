using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 플레이어의 스킬 트리 런타임 상태를 관리하는 컴포넌트.
/// 해금 상태를 Dictionary로 보관하여 ScriptableObject 에셋 오염을 방지한다.
/// 트리 데이터는 GameManager의 선택된 직업을 우선 사용하고,
/// GameManager가 없으면 직렬화된 treeData로 폴백한다(테스트 씬 호환).
/// </summary>
[RequireComponent(typeof(Player))]
public class SkillTreeComponent : MonoBehaviour
{
    [SerializeField]
    [Tooltip("GameManager가 없을 때 사용할 폴백 스킬 트리(테스트용)")]
    private SkillTreeData treeData;
    public SkillTreeData TreeData => treeData;

    // ScriptableObject를 직접 수정하지 않고 런타임 상태를 Dictionary로 격리
    private Dictionary<SkillNodeData, bool> unlockedState;

    // 장착된 스킬 슬롯 (null = 비어있음)
    private List<ActiveSkillData> equippedActives;
    private List<PassiveSkillData> equippedPassives;
    private readonly List<EntityStatModifierHandle> passiveModifierHandles = new();
    private readonly List<PassiveSkillRuntime> passiveRuntimes = new();

    private ISkillTreeStrategy strategy;
    private Entity entity;

    private void Awake()
    {
        entity = GetComponent<Entity>();

        ResolveTreeData();

        strategy = treeData != null ? treeData.CreateStrategy() : null;

        int activeSlots = treeData != null ? treeData.MaxActiveSkillSlots : 5;
        int passiveSlots = treeData != null ? treeData.MaxPassiveSkillSlots : 3;

        var gm = GameManager.InstanceOrNull;
        if (gm != null)
        {
            Career currentJob = gm.SelectedJob;
            unlockedState = gm.GetUnlockedState(currentJob);
            equippedActives = gm.GetEquippedActives(currentJob, activeSlots);
            equippedPassives = gm.GetEquippedPassives(currentJob, passiveSlots);
        }
        else
        {
            // 테스트 씬 등 GameManager가 없는 경우를 위한 폴백
            unlockedState = new Dictionary<SkillNodeData, bool>();
            equippedActives = new List<ActiveSkillData>(new ActiveSkillData[activeSlots]);
            equippedPassives = new List<PassiveSkillData>(new PassiveSkillData[passiveSlots]);
        }

        if (treeData == null)
        {
            Debug.LogWarning("[SkillTreeComponent] SkillTreeData가 비어 있습니다!");
        }

        // 패시브 스탯 재생성 복구 (GameManager에서 가져온 기존 장착 데이터 적용)
        RecalculatePassives();
    }

    private void OnDestroy()
    {
        ClearPassiveEffects();
    }

    /// <summary>
    /// 트리 데이터를 결정한다. GameManager가 존재하고 선택된 직업의 트리가
    /// 등록되어 있으면 그것을 사용하고, 아니면 직렬화된 treeData를 유지한다.
    /// </summary>
    private void ResolveTreeData()
    {
        var gm = GameManager.InstanceOrNull;
        if (gm == null)
            return;

        var selectedTree = gm.CurrentSkillTreeData;
        if (selectedTree != null)
            treeData = selectedTree;
    }

    // ─── 해금 흐름 ───────────────────────────────────────────────

    /// <summary>
    /// UI에서 노드 선택 시 진입점. SP 소비 + 전략 조건 모두 통과해야 해금된다.
    /// </summary>
    public bool TryUnlockNode(SkillNodeData node)
    {
        if (node == null || strategy == null)
            return false;

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
        GoodsManager.Instance.GetGoods(GoodsType.SP).Decrease((uint)node.Skill.SPCost);

        unlockedState[node] = true;
        strategy.OnNodeUnlocked(node, unlockedState);

        Debug.Log($"[SkillTree] {node.Skill?.SkillName} 해금 완료.");
        return true;
    }

    /// <summary>
    /// 장착된 패시브 스킬 효과만 재계산하여 Entity 스탯에 반영한다.
    /// (해금만으로는 효과가 적용되지 않으며, 슬롯에 장착해야 적용된다.)
    /// </summary>
    private void RecalculatePassives()
    {
        ClearPassiveEffects();

        foreach (PassiveSkillData passive in equippedPassives)
        {
            if (passive == null)
                continue;

            EntityStatModifier modifier = EntityStatModifier.Zero;
            passive.ApplyPassive(ref modifier);
            passiveModifierHandles.Add(entity.ApplyStatModifier(modifier));

            PassiveSkillRuntime runtime = passive.CreateRuntime(
                new PassiveSkillRuntimeContext(this, entity)
            );
            if (runtime != null)
                passiveRuntimes.Add(runtime);
        }
    }

    public float GetActiveSkillCooldownMultiplier()
    {
        if (equippedPassives == null)
            return 1f;

        float reduction = 0f;
        foreach (PassiveSkillData passive in equippedPassives)
        {
            if (passive is IActiveSkillCooldownModifier cooldownModifier)
                reduction += cooldownModifier.CooldownReductionRate;
        }

        return Mathf.Max(0f, 1f - reduction);
    }

    private void ClearPassiveEffects()
    {
        foreach (PassiveSkillRuntime runtime in passiveRuntimes)
            runtime.Dispose();
        passiveRuntimes.Clear();

        foreach (EntityStatModifierHandle handle in passiveModifierHandles)
        {
            if (entity != null)
                entity.RemoveStatModifier(handle);
        }
        passiveModifierHandles.Clear();
    }

    // ─── 액티브 스킬 장착 ─────────────────────────────────────────

    /// <summary>
    /// 해금된 액티브 스킬을 지정 슬롯에 장착한다.
    /// </summary>
    public bool TryEquipActiveSkill(ActiveSkillData skill, int slotIndex)
    {
        if (skill == null || slotIndex < 0 || slotIndex >= equippedActives.Count)
            return false;

        if (!IsUnlocked(FindNodeBySkill(skill)))
        {
            Debug.Log($"[SkillTree] {skill.SkillName}이 해금되지 않았습니다.");
            return false;
        }

        equippedActives[slotIndex] = skill;
        return true;
    }

    public void UnequipActiveSkill(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < equippedActives.Count)
            equippedActives[slotIndex] = null;
    }

    public ActiveSkillData GetEquippedActive(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedActives.Count)
            return null;
        return equippedActives[slotIndex];
    }

    public IReadOnlyList<ActiveSkillData> GetEquippedActives() => equippedActives.AsReadOnly();

    // ─── 패시브 스킬 장착 ─────────────────────────────────────────

    /// <summary>
    /// 해금된 패시브 스킬을 지정 슬롯에 장착하고 스탯에 반영한다.
    /// </summary>
    public bool TryEquipPassiveSkill(PassiveSkillData skill, int slotIndex)
    {
        if (skill == null || slotIndex < 0 || slotIndex >= equippedPassives.Count)
            return false;

        if (!IsUnlocked(FindNodeBySkill(skill)))
        {
            Debug.Log($"[SkillTree] {skill.SkillName}이 해금되지 않았습니다.");
            return false;
        }

        equippedPassives[slotIndex] = skill;
        RecalculatePassives();
        return true;
    }

    public void UnequipPassiveSkill(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < equippedPassives.Count)
        {
            equippedPassives[slotIndex] = null;
            RecalculatePassives();
        }
    }

    public PassiveSkillData GetEquippedPassive(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedPassives.Count)
            return null;
        return equippedPassives[slotIndex];
    }

    public IReadOnlyList<PassiveSkillData> GetEquippedPassives() => equippedPassives.AsReadOnly();

    // ─── 조회 ────────────────────────────────────────────────────

    public bool IsUnlocked(SkillNodeData node)
    {
        return node != null && unlockedState.TryGetValue(node, out bool v) && v;
    }

    /// <summary>
    /// SP 조건과 전략 조건을 통합하여 해금 가능 여부를 반환한다.
    /// </summary>
    public bool CanUnlock(SkillNodeData node)
    {
        if (node == null || node.Skill == null || strategy == null)
            return false;
        var spGoods = GoodsManager.Instance.GetGoods(GoodsType.SP) as SO_SP;
        if (strategy == null || spGoods == null || !SkillTreeValidator.HasEnoughSP(node, spGoods))
            return false;
        return strategy.CanUnlock(node, unlockedState);
    }

    // ─── 내부 유틸 ────────────────────────────────────────────────

    private SkillNodeData FindNodeBySkill(SkillData skill)
    {
        return treeData?.AllNodes?.FirstOrDefault(n => n.Skill == skill);
    }
}
