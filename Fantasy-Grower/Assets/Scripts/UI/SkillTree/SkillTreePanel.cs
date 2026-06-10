using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 트리 전체 UI를 조율하는 패널.
/// BuildTree()로 노드 버튼을 동적 생성하고 RefreshAll()로 상태를 동기화한다.
/// </summary>
public class SkillTreePanel : MonoBehaviour
{
    [Header("스킬 트리 데이터")]
    [SerializeField]
    private SkillTreeComponent skillTreeComponent;

    [Header("노드 버튼 생성")]
    [SerializeField]
    private GameObject skillNodeButtonPrefab;

    [SerializeField]
    private RectTransform nodeContainer;

    [Header("장착 슬롯")]
    [SerializeField]
    private SkillEquipSlotUI[] equipSlotUIs;

    [Header("텍스트")]
    [SerializeField]
    private Text spText;

    [SerializeField]
    private Text entityStatsText;

    [Header("레이아웃")]
    [SerializeField]
    private float nodeSpacingX = 120f;

    [SerializeField]
    private float nodeSpacingY = 100f;

    private readonly List<SkillNodeButton> nodeButtons = new();
    private ActiveSkillData selectedActiveSkill;

    private void Start()
    {
        var sp = GoodsManager.Instance.GetGoods(GoodsType.SP);
        if (sp != null)
            sp.OnValueChange += OnSPChanged;

        for (int i = 0; i < equipSlotUIs.Length; i++)
            equipSlotUIs[i].Initialize(i, this);

        BuildTree();
        RefreshAll();
    }

    private void OnDestroy()
    {
        var sp = GoodsManager.Instance.GetGoods(GoodsType.SP);
        if (sp != null)
            sp.OnValueChange -= OnSPChanged;
    }

    private void OnSPChanged(uint value) => RefreshAll();

    // ─── 트리 구축 ───────────────────────────────────────────────────

    private void BuildTree()
    {
        foreach (var btn in nodeButtons)
            if (btn != null)
                Destroy(btn.gameObject);
        nodeButtons.Clear();

        if (skillTreeComponent == null || skillTreeComponent.TreeData == null)
        {
            Debug.LogWarning("[SkillTreePanel] SkillTreeComponent 또는 TreeData가 없습니다.");
            return;
        }

        var allNodes = skillTreeComponent.TreeData.AllNodes;
        if (allNodes == null)
            return;

        foreach (var node in allNodes)
        {
            if (node == null || node.Skill == null)
                continue;

            var go = Instantiate(skillNodeButtonPrefab, nodeContainer);
            var btn = go.GetComponent<SkillNodeButton>();
            if (btn == null)
                continue;

            btn.Initialize(node, this);
            var rt = go.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = new Vector2(
                    node.SlotIndex * nodeSpacingX,
                    -node.TierIndex * nodeSpacingY
                );

            nodeButtons.Add(btn);
        }
    }

    // ─── 갱신 ────────────────────────────────────────────────────────

    public void RefreshAll()
    {
        RefreshSPText();
        RefreshEntityStats();
        RefreshNodeButtons();
        RefreshEquipSlots();
    }

    private void RefreshSPText()
    {
        if (spText == null)
            return;
        var sp = GoodsManager.Instance.GetGoods(GoodsType.SP);
        spText.text = sp != null ? $"SP: {sp.Get()}" : "SP: -";
    }

    private void RefreshEntityStats()
    {
        if (entityStatsText == null || skillTreeComponent == null)
            return;
        var entity = skillTreeComponent.GetComponent<Entity>();
        if (entity == null)
            return;

        entityStatsText.text =
            $"HP: {entity.Hp} / {entity.MaxHp}\n"
            + $"HP Regen: {entity.HpRecovery:F1}\n"
            + $"ATK: {entity.AttackPower}\n"
            + $"ATK Speed: {entity.AttackSpeed:F2}\n"
            + $"Crit%: {entity.CriticalPercentage:F1}";
    }

    private void RefreshNodeButtons()
    {
        if (skillTreeComponent?.TreeData?.AllNodes == null)
            return;
        var allNodes = skillTreeComponent.TreeData.AllNodes;

        for (int i = 0; i < nodeButtons.Count && i < allNodes.Count; i++)
        {
            var node = allNodes[i];
            var btn = nodeButtons[i];
            if (node == null || btn == null)
                continue;

            bool isUnlocked = skillTreeComponent.IsUnlocked(node);
            bool canUnlock = skillTreeComponent.CanUnlock(node);
            bool isSelected = node.Skill is ActiveSkillData active && active == selectedActiveSkill;

            btn.Refresh(isUnlocked, canUnlock, isSelected);
        }
    }

    private void RefreshEquipSlots()
    {
        if (equipSlotUIs == null)
            return;
        var equipped = skillTreeComponent?.GetEquippedActives();

        for (int i = 0; i < equipSlotUIs.Length; i++)
        {
            ActiveSkillData skill = (equipped != null && i < equipped.Count) ? equipped[i] : null;
            equipSlotUIs[i].Refresh(skill);
        }
    }

    // ─── 이벤트 핸들러 ───────────────────────────────────────────────

    /// <summary>
    /// SkillNodeButton 클릭 시 호출된다.
    /// 미해금 → TryUnlockNode. 해금된 액티브 → 장착 대기 선택/해제.
    /// </summary>
    public void OnNodeClicked(SkillNodeData node)
    {
        if (node == null || skillTreeComponent == null)
            return;

        if (!skillTreeComponent.IsUnlocked(node))
        {
            skillTreeComponent.TryUnlockNode(node);
        }
        else if (node.Skill is ActiveSkillData activeSkill)
        {
            selectedActiveSkill = selectedActiveSkill == activeSkill ? null : activeSkill;
        }

        RefreshAll();
    }

    /// <summary>
    /// SkillEquipSlotUI 클릭 시 호출된다.
    /// 선택된 액티브 스킬이 있으면 해당 슬롯에 장착하고 선택을 초기화한다.
    /// </summary>
    public void OnEquipSlotClicked(int slotIndex)
    {
        if (selectedActiveSkill == null || skillTreeComponent == null)
            return;
        skillTreeComponent.TryEquipActiveSkill(selectedActiveSkill, slotIndex);
        selectedActiveSkill = null;
        RefreshAll();
    }

    // ─── 테스트 유틸 ─────────────────────────────────────────────────

    /// <summary>
    /// Inspector Button의 OnClick()에 연결해서 테스트용 SP를 추가한다.
    /// </summary>
    public void AddTestSP(int amount)
    {
        if (amount <= 0)
            return;
        GoodsManager.Instance.GetGoods(GoodsType.SP)?.Increase((uint)amount);
    }
}
