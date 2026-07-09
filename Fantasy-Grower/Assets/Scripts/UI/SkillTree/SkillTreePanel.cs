using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 트리 전체 UI를 조율하는 패널.
/// BuildTree()로 노드 버튼을 동적 생성하고 RefreshAll()로 상태를 동기화한다.
/// 액티브/패시브 장착 슬롯을 각각 관리한다.
/// </summary>
public class SkillTreePanel : MonoBehaviour
{
    private enum NodeAlignment
    {
        Left,
        CenterByTier,
    }

    private struct SlotIndexRange
    {
        public float Minimum;
        public float Maximum;

        public float Center => (Minimum + Maximum) * 0.5f;

        public SlotIndexRange(float slotIndex)
        {
            Minimum = slotIndex;
            Maximum = slotIndex;
        }

        public void Include(float slotIndex)
        {
            if (slotIndex < Minimum)
                Minimum = slotIndex;
            if (slotIndex > Maximum)
                Maximum = slotIndex;
        }
    }

    [Header("스킬 트리 데이터")]
    [SerializeField]
    [Tooltip("GameManager가 없는 테스트 씬에서만 사용할 폴백 SkillTreeComponent")]
    private SkillTreeComponent fallbackSkillTreeComponent;

    [Header("노드 버튼 생성")]
    [SerializeField]
    private GameObject skillNodeButtonPrefab;

    [SerializeField]
    private RectTransform nodeContainer;

    [Header("장착 슬롯")]
    [SerializeField]
    private SkillEquipSlotUI[] activeEquipSlotUIs;

    [SerializeField]
    private SkillEquipSlotUI[] passiveEquipSlotUIs;

    [Header("텍스트")]
    [SerializeField]
    private Text spText;

    [SerializeField]
    private Text entityStatsText;

    [Header("스킬 상세")]
    [SerializeField]
    private GameObject skillDetailPanel;

    [SerializeField]
    private TMP_Text detailSkillNameText;

    [SerializeField]
    private Image detailSkillIconImage;

    [SerializeField]
    private TMP_Text detailSkillTypeText;

    [SerializeField]
    private TMP_Text detailSkillDescriptionText;

    [SerializeField]
    private Button unlockSkillButton;

    [SerializeField]
    private Button equipSkillButton;

    [Header("레이아웃")]
    [SerializeField]
    private float nodeSpacingX = 120f;

    [SerializeField]
    private float nodeSpacingY = 100f;

    [SerializeField]
    private NodeAlignment nodeAlignment = NodeAlignment.CenterByTier;

    [SerializeField]
    private Vector2 nodeOffset;

    [Header("스크롤 영역")]
    [SerializeField]
    private bool autoResizeNodeContainer = true;

    [SerializeField]
    private Vector2 nodeContainerPadding = new(80f, 80f);

    [SerializeField]
    private Vector2 minimumNodeContainerSize = new(0f, 0f);

    [Header("연결선")]
    [SerializeField]
    private RectTransform connectionContainer;

    [SerializeField]
    private Image connectionLinePrefab;

    [SerializeField]
    private Color connectionLineColor = Color.white;

    [SerializeField]
    [Min(1f)]
    private float connectionLineThickness = 4f;

    private readonly List<SkillNodeButton> nodeButtons = new();
    private readonly List<RectTransform> connectionLines = new();
    private readonly Dictionary<SkillNodeData, RectTransform> nodeRects = new();
    private readonly Dictionary<SkillNodeData, SkillNodeButton> nodeButtonMap = new();
    private readonly Dictionary<int, SlotIndexRange> tierSlotIndexRanges = new();
    private readonly Dictionary<SkillNodeData, float> nodeLayoutSlots = new();
    private readonly Dictionary<string, int> attributeLaneIndexes = new();

    // 장착 대기로 선택된 스킬 (카테고리별로 하나씩)
    private SkillNodeData focusedNode;
    private ActiveSkillData selectedActiveSkill;
    private PassiveSkillData selectedPassiveSkill;
    private SkillTreeComponent skillTreeComponent;
    private GameManager boundGameManager;
    private bool equipSlotsInitialized;
    private float attributeLaneSlotWidth = 1f;

    private void Awake()
    {
        InitializeEquipSlots();
        InitializeDetailPanel();
    }

    private void OnEnable()
    {
        SubscribeSP();
        BindGameManager();
        TryBindSkillTreeComponent();
        RefreshAll();
    }

    private void LateUpdate()
    {
        if (skillTreeComponent != null)
            return;

        BindGameManager();
        if (TryBindSkillTreeComponent())
            RefreshAll();
    }

    private void OnDisable()
    {
        UnbindGameManager();
        UnsubscribeSP();
    }

    private void OnDestroy()
    {
        if (unlockSkillButton != null)
            unlockSkillButton.onClick.RemoveListener(OnUnlockButtonClicked);
        if (equipSkillButton != null)
            equipSkillButton.onClick.RemoveListener(OnEquipButtonClicked);
    }

    private void InitializeEquipSlots()
    {
        if (equipSlotsInitialized)
            return;

        for (int i = 0; i < activeEquipSlotUIs.Length; i++)
            activeEquipSlotUIs[i].Initialize(i, this, SkillCategory.Active);
        for (int i = 0; i < passiveEquipSlotUIs.Length; i++)
            passiveEquipSlotUIs[i].Initialize(i, this, SkillCategory.Passive);

        equipSlotsInitialized = true;
    }

    private void InitializeDetailPanel()
    {
        if (unlockSkillButton != null)
            unlockSkillButton.onClick.AddListener(OnUnlockButtonClicked);
        if (equipSkillButton != null)
            equipSkillButton.onClick.AddListener(OnEquipButtonClicked);

        SetSkillDetailPanelActive(false);
    }

    private void SubscribeSP()
    {
        var sp = GoodsManager.Instance.GetGoods(GoodsType.SP);
        if (sp != null)
            sp.OnValueChange += OnSPChanged;
    }

    private void UnsubscribeSP()
    {
        var sp = GoodsManager.Instance.GetGoods(GoodsType.SP);
        if (sp != null)
            sp.OnValueChange -= OnSPChanged;
    }

    private void OnSPChanged(uint value) => RefreshAll();

    private void BindGameManager()
    {
        GameManager foundGameManager = GameManager.InstanceOrNull;
        if (foundGameManager == boundGameManager)
            return;

        UnbindGameManager();
        boundGameManager = foundGameManager;

        if (boundGameManager != null)
            boundGameManager.OnPlayerChanged += HandlePlayerChanged;
    }

    private void UnbindGameManager()
    {
        if (boundGameManager == null)
            return;

        boundGameManager.OnPlayerChanged -= HandlePlayerChanged;
        boundGameManager = null;
    }

    private void HandlePlayerChanged(Entity player)
    {
        BindSkillTreeFromPlayer(player);
    }

    private bool TryBindSkillTreeComponent()
    {
        if (boundGameManager != null)
        {
            Entity player = boundGameManager.GetPlayer();
            if (BindSkillTreeFromPlayer(player))
                return true;
        }

        if (fallbackSkillTreeComponent != null)
            return BindSkillTreeComponent(fallbackSkillTreeComponent);

        return false;
    }

    private bool BindSkillTreeFromPlayer(Entity player)
    {
        if (player == null)
            return false;

        if (!player.TryGetComponent(out SkillTreeComponent foundSkillTreeComponent))
            return false;

        return BindSkillTreeComponent(foundSkillTreeComponent);
    }

    private bool BindSkillTreeComponent(SkillTreeComponent foundSkillTreeComponent)
    {
        if (foundSkillTreeComponent == null)
            return false;

        if (foundSkillTreeComponent == skillTreeComponent)
            return true;

        skillTreeComponent = foundSkillTreeComponent;
        focusedNode = null;
        selectedActiveSkill = null;
        selectedPassiveSkill = null;

        BuildTree();
        RefreshAll();
        return true;
    }

    // ─── 트리 구축 ───────────────────────────────────────────────────

    private void BuildTree()
    {
        ClearTreeVisuals();

        if (skillTreeComponent == null || skillTreeComponent.TreeData == null)
        {
            Debug.LogWarning("[SkillTreePanel] SkillTreeComponent 또는 TreeData가 없습니다.");
            return;
        }

        var allNodes = skillTreeComponent.TreeData.AllNodes;
        if (allNodes == null)
            return;

        CacheTierSlotIndexRanges(allNodes);
        PrepareNodeContainerForTreeLayout();

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
            {
                PrepareNodeRectForTreeLayout(rt);
                rt.anchoredPosition = CalculateNodePosition(node);
                nodeRects[node] = rt;
            }

            nodeButtons.Add(btn);
            nodeButtonMap[node] = btn;
        }

        FitNodeContainerToNodes();
        BuildConnectionLines(allNodes);
    }

    private void ClearTreeVisuals()
    {
        for (int i = 0; i < nodeButtons.Count; i++)
            if (nodeButtons[i] != null)
                Destroy(nodeButtons[i].gameObject);

        for (int i = 0; i < connectionLines.Count; i++)
            if (connectionLines[i] != null)
                Destroy(connectionLines[i].gameObject);

        nodeButtons.Clear();
        connectionLines.Clear();
        nodeRects.Clear();
        nodeButtonMap.Clear();
    }

    private void CacheTierSlotIndexRanges(IReadOnlyList<SkillNodeData> allNodes)
    {
        tierSlotIndexRanges.Clear();
        nodeLayoutSlots.Clear();
        CacheAttributeLaneIndexes(allNodes);

        for (int i = 0; i < allNodes.Count; i++)
        {
            SkillNodeData node = allNodes[i];
            if (node == null || node.Skill == null)
                continue;

            float layoutSlot = CalculateLayoutSlot(node);
            nodeLayoutSlots[node] = layoutSlot;

            if (tierSlotIndexRanges.TryGetValue(node.TierIndex, out SlotIndexRange range))
            {
                range.Include(layoutSlot);
                tierSlotIndexRanges[node.TierIndex] = range;
            }
            else
            {
                tierSlotIndexRanges.Add(node.TierIndex, new SlotIndexRange(layoutSlot));
            }
        }
    }

    private void CacheAttributeLaneIndexes(IReadOnlyList<SkillNodeData> allNodes)
    {
        attributeLaneIndexes.Clear();
        int maximumTaggedSlotIndex = 0;

        for (int i = 0; i < allNodes.Count; i++)
        {
            SkillNodeData node = allNodes[i];
            if (node == null || node.Skill == null || string.IsNullOrEmpty(node.AttributeTag))
                continue;

            if (!attributeLaneIndexes.ContainsKey(node.AttributeTag))
                attributeLaneIndexes.Add(node.AttributeTag, attributeLaneIndexes.Count);

            if (node.SlotIndex > maximumTaggedSlotIndex)
                maximumTaggedSlotIndex = node.SlotIndex;
        }

        // 속성 레인 안에서 SlotIndex가 갈라지는 티어가 있어도 옆 속성과 겹치지 않도록 간격을 잡습니다.
        attributeLaneSlotWidth = Mathf.Max(1f, maximumTaggedSlotIndex + 2f);
    }

    private float CalculateLayoutSlot(SkillNodeData node)
    {
        if (
            attributeLaneIndexes.Count > 1
            && !string.IsNullOrEmpty(node.AttributeTag)
            && attributeLaneIndexes.TryGetValue(node.AttributeTag, out int laneIndex)
        )
        {
            float centeredLaneIndex = laneIndex - (attributeLaneIndexes.Count - 1) * 0.5f;
            return centeredLaneIndex * attributeLaneSlotWidth + node.SlotIndex;
        }

        return node.SlotIndex;
    }

    private Vector2 CalculateNodePosition(SkillNodeData node)
    {
        float slotPosition = nodeLayoutSlots.TryGetValue(node, out float layoutSlot)
            ? layoutSlot
            : node.SlotIndex;

        if (
            nodeAlignment == NodeAlignment.CenterByTier
            && tierSlotIndexRanges.TryGetValue(node.TierIndex, out SlotIndexRange range)
        )
        {
            slotPosition -= range.Center;
        }

        return new Vector2(slotPosition * nodeSpacingX, -node.TierIndex * nodeSpacingY)
            + nodeOffset;
    }

    private void FitNodeContainerToNodes()
    {
        if (!autoResizeNodeContainer || nodeContainer == null || nodeRects.Count == 0)
            return;

        if (!TryCalculateNodeBounds(out Vector2 min, out Vector2 max))
            return;

        Vector2 shift = CalculateBoundsShift(min, max);
        if (shift != Vector2.zero)
            ShiftNodeRects(shift);

        if (!TryCalculateNodeBounds(out min, out max))
            return;

        Vector2 contentSize = max - min + nodeContainerPadding * 2f;
        contentSize.x = Mathf.Max(contentSize.x, minimumNodeContainerSize.x);
        contentSize.y = Mathf.Max(contentSize.y, minimumNodeContainerSize.y);

        ResizeNodeContainer(contentSize);
    }

    private void PrepareNodeContainerForTreeLayout()
    {
        if (nodeContainer == null)
            return;

        // ScrollView Content는 좌상단 기준으로 커져야 노드와 연결선 좌표가 흔들리지 않습니다.
        SetPivotWithoutMovingRect(nodeContainer, new Vector2(0f, 1f));
    }

    private void PrepareNodeRectForTreeLayout(RectTransform nodeRect)
    {
        nodeRect.anchorMin = new Vector2(0f, 1f);
        nodeRect.anchorMax = new Vector2(0f, 1f);
        nodeRect.pivot = new Vector2(0.5f, 0.5f);
    }

    private void SetPivotWithoutMovingRect(RectTransform rectTransform, Vector2 pivot)
    {
        if (rectTransform.pivot == pivot)
            return;

        Vector2 size = rectTransform.rect.size;
        Vector2 pivotDelta = pivot - rectTransform.pivot;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition += new Vector2(pivotDelta.x * size.x, pivotDelta.y * size.y);
    }

    private void ResizeNodeContainer(Vector2 contentSize)
    {
        // stretch 앵커에서도 실제 Rect 크기를 기준으로 조정합니다.
        nodeContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentSize.x);
        nodeContainer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentSize.y);
    }

    private bool TryCalculateNodeBounds(out Vector2 min, out Vector2 max)
    {
        min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        bool hasBounds = false;

        foreach (RectTransform nodeRect in nodeRects.Values)
        {
            if (nodeRect == null)
                continue;

            GetNodeRectBounds(nodeRect, out Vector2 nodeMin, out Vector2 nodeMax);
            min = Vector2.Min(min, nodeMin);
            max = Vector2.Max(max, nodeMax);
            hasBounds = true;
        }

        return hasBounds;
    }

    private void GetNodeRectBounds(RectTransform nodeRect, out Vector2 nodeMin, out Vector2 nodeMax)
    {
        Rect rect = nodeRect.rect;
        Vector2 position = nodeRect.anchoredPosition;
        nodeMin = position + rect.min;
        nodeMax = position + rect.max;
    }

    private Vector2 CalculateBoundsShift(Vector2 min, Vector2 max)
    {
        Vector2 shift = Vector2.zero;

        if (min.x < nodeContainerPadding.x)
            shift.x = nodeContainerPadding.x - min.x;
        if (max.y > -nodeContainerPadding.y)
            shift.y = -nodeContainerPadding.y - max.y;

        return shift;
    }

    private void ShiftNodeRects(Vector2 shift)
    {
        foreach (RectTransform nodeRect in nodeRects.Values)
        {
            if (nodeRect != null)
                nodeRect.anchoredPosition += shift;
        }
    }

    private void BuildConnectionLines(IReadOnlyList<SkillNodeData> allNodes)
    {
        RectTransform lineParent = GetConnectionParent();
        if (lineParent == null)
            return;

        PrepareConnectionParentForTreeLayout(lineParent);

        for (int i = 0; i < allNodes.Count; i++)
        {
            SkillNodeData node = allNodes[i];
            if (node == null || node.Prerequisites == null)
                continue;

            if (!nodeRects.TryGetValue(node, out RectTransform targetRect))
                continue;

            for (int j = 0; j < node.Prerequisites.Length; j++)
            {
                SkillNodeData prerequisite = node.Prerequisites[j];
                if (prerequisite == null)
                    continue;

                if (!nodeRects.TryGetValue(prerequisite, out RectTransform sourceRect))
                    continue;

                CreateConnectionLine(
                    lineParent,
                    GetLocalCenter(lineParent, sourceRect),
                    GetLocalCenter(lineParent, targetRect)
                );
            }
        }
    }

    private RectTransform GetConnectionParent()
    {
        if (connectionContainer != null)
            return connectionContainer;

        return nodeContainer;
    }

    private void PrepareConnectionParentForTreeLayout(RectTransform lineParent)
    {
        // 연결선도 노드와 같은 좌상단 좌표계에서 그려야 중심점 계산이 일치합니다.
        SetPivotWithoutMovingRect(lineParent, new Vector2(0f, 1f));
    }

    private Vector2 GetLocalCenter(RectTransform parent, RectTransform rect)
    {
        return parent.InverseTransformPoint(rect.TransformPoint(rect.rect.center));
    }

    private void CreateConnectionLine(RectTransform lineParent, Vector2 from, Vector2 to)
    {
        RectTransform lineRect;
        Image lineImage;

        if (connectionLinePrefab != null)
        {
            lineImage = Instantiate(connectionLinePrefab, lineParent);
            lineRect = lineImage.rectTransform;
        }
        else
        {
            var lineObject = new GameObject(
                "SkillConnectionLine",
                typeof(RectTransform),
                typeof(Image)
            );
            lineObject.transform.SetParent(lineParent, false);
            lineRect = lineObject.GetComponent<RectTransform>();
            lineImage = lineObject.GetComponent<Image>();
        }

        Vector2 delta = to - from;
        lineRect.anchorMin = new Vector2(0f, 1f);
        lineRect.anchorMax = new Vector2(0f, 1f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.anchoredPosition = from + delta * 0.5f;
        lineRect.sizeDelta = new Vector2(delta.magnitude, connectionLineThickness);
        lineRect.localRotation = Quaternion.Euler(
            0f,
            0f,
            Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg
        );
        lineRect.SetAsFirstSibling();

        lineImage.color = connectionLineColor;
        lineImage.raycastTarget = false;
        connectionLines.Add(lineRect);
    }

    // ─── 갱신 ────────────────────────────────────────────────────────

    public void RefreshAll()
    {
        RefreshSPText();
        RefreshEntityStats();
        RefreshNodeButtons();
        RefreshEquipSlots();
        RefreshSkillDetailPanel();
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
        if (
            skillTreeComponent == null
            || skillTreeComponent.TreeData == null
            || skillTreeComponent.TreeData.AllNodes == null
        )
            return;
        foreach (var kvp in nodeButtonMap)
        {
            var node = kvp.Key;
            var btn = kvp.Value;
            if (node == null || btn == null)
                continue;

            bool isUnlocked = skillTreeComponent.IsUnlocked(node);
            bool isEquipped = skillTreeComponent.IsEquipped(node.Skill);

            btn.Refresh(isUnlocked, isEquipped);
        }
    }

    private bool IsNodeSelected(SkillNodeData node)
    {
        if (node == focusedNode)
            return true;

        return node.Skill switch
        {
            ActiveSkillData active => active == selectedActiveSkill,
            PassiveSkillData passive => passive == selectedPassiveSkill,
            _ => false,
        };
    }

    private void RefreshSkillDetailPanel()
    {
        if (skillDetailPanel == null)
            return;

        if (focusedNode == null || focusedNode.Skill == null)
        {
            SetSkillDetailPanelActive(false);
            return;
        }

        SetSkillDetailPanelActive(true);

        SkillData skill = focusedNode.Skill;
        RefreshDetailSkillIcon(skill);

        if (detailSkillNameText != null)
            detailSkillNameText.text = skill.SkillName;
        if (detailSkillTypeText != null)
            detailSkillTypeText.text = GetSkillTypeText(skill);
        if (detailSkillDescriptionText != null)
            detailSkillDescriptionText.text = skill.SkillDescription;

        bool canShowUnlockButton =
            skillTreeComponent != null
            && !skillTreeComponent.IsUnlocked(focusedNode)
            && skillTreeComponent.CanUnlock(focusedNode);

        if (unlockSkillButton != null)
            unlockSkillButton.gameObject.SetActive(canShowUnlockButton);

        bool canShowEquipButton =
            skillTreeComponent != null
            && skillTreeComponent.IsUnlocked(focusedNode)
            && IsEquippableSkill(skill);

        if (equipSkillButton != null)
            equipSkillButton.gameObject.SetActive(canShowEquipButton);
    }

    private void RefreshDetailSkillIcon(SkillData skill)
    {
        if (detailSkillIconImage == null)
            return;

        Sprite icon = skill != null ? skill.SkillIcon : null;
        detailSkillIconImage.sprite = icon;
        detailSkillIconImage.preserveAspect = true;
        detailSkillIconImage.gameObject.SetActive(icon != null);
    }

    private void SetSkillDetailPanelActive(bool active)
    {
        if (skillDetailPanel != null && skillDetailPanel.activeSelf != active)
            skillDetailPanel.SetActive(active);
    }

    private string GetSkillTypeText(SkillData skill)
    {
        if (skill is BasicAttackSkillData)
            return "평타";
        if (skill is ActiveSkillData)
            return "액티브";
        if (skill is PassiveSkillData)
            return "패시브";

        return "스킬";
    }

    private bool IsEquippableSkill(SkillData skill)
    {
        return skill is ActiveSkillData || skill is PassiveSkillData;
    }

    private void RefreshEquipSlots()
    {
        RefreshSlotCategory(
            activeEquipSlotUIs,
            skillTreeComponent != null ? skillTreeComponent.GetEquippedActives() : null,
            selectedActiveSkill != null
        );
        RefreshSlotCategory(
            passiveEquipSlotUIs,
            skillTreeComponent != null ? skillTreeComponent.GetEquippedPassives() : null,
            selectedPassiveSkill != null
        );
    }

    private void RefreshSlotCategory(
        IReadOnlyList<SkillEquipSlotUI> slots,
        IReadOnlyList<SkillData> skills,
        bool shouldHighlight
    )
    {
        for (int i = 0; i < slots.Count; i++)
        {
            SkillData skill = (skills != null && i < skills.Count) ? skills[i] : null;
            slots[i].Refresh(skill, shouldHighlight);
        }
    }

    // ─── 이벤트 핸들러 ───────────────────────────────────────────────

    /// <summary>
    /// SkillNodeButton 클릭 시 호출된다.
    /// 노드 상세 패널을 열고, 해금/장착 액션은 상세 패널 버튼에서 처리한다.
    /// </summary>
    public void OnNodeClicked(SkillNodeData node)
    {
        if (node == null || skillTreeComponent == null)
            return;

        focusedNode = node;
        selectedActiveSkill = null;
        selectedPassiveSkill = null;

        RefreshAll();
    }

    private void OnUnlockButtonClicked()
    {
        if (focusedNode == null || skillTreeComponent == null)
            return;

        if (!skillTreeComponent.TryUnlockNode(focusedNode))
        {
            RefreshAll();
            return;
        }

        if (focusedNode.Skill is BasicAttackSkillData)
        {
            SetSkillDetailPanelActive(false);
            focusedNode = null;
        }

        RefreshAll();
    }

    private void OnEquipButtonClicked()
    {
        if (
            focusedNode == null
            || focusedNode.Skill == null
            || skillTreeComponent == null
            || !skillTreeComponent.IsUnlocked(focusedNode)
            || !IsEquippableSkill(focusedNode.Skill)
        )
            return;

        SelectUnlockedSkillForEquip(focusedNode);
        SetSkillDetailPanelActive(false);
        focusedNode = null;
        RefreshAll();
    }

    private void SelectUnlockedSkillForEquip(SkillNodeData node)
    {
        if (node == null || node.Skill == null)
            return;

        if (node.Skill is BasicAttackSkillData)
        {
            selectedActiveSkill = null;
            selectedPassiveSkill = null;
        }
        else if (node.Skill is ActiveSkillData activeSkill)
        {
            selectedActiveSkill = activeSkill;
            selectedPassiveSkill = null;
        }
        else if (node.Skill is PassiveSkillData passiveSkill)
        {
            selectedPassiveSkill = passiveSkill;
            selectedActiveSkill = null;
        }
    }

    /// <summary>
    /// SkillEquipSlotUI 클릭 시 호출된다.
    /// 슬롯 카테고리에 맞는 선택 스킬이 있으면 장착하고 선택을 초기화한다.
    /// </summary>
    public void OnEquipSlotClicked(int slotIndex, SkillCategory category)
    {
        if (skillTreeComponent == null)
            return;

        if (category == SkillCategory.Active)
        {
            if (selectedActiveSkill == null)
                return;

            if (skillTreeComponent.TryEquipActiveSkill(selectedActiveSkill, slotIndex))
                selectedActiveSkill = null;
        }
        else // Passive
        {
            if (selectedPassiveSkill == null)
                return;

            if (skillTreeComponent.TryEquipPassiveSkill(selectedPassiveSkill, slotIndex))
                selectedPassiveSkill = null;
        }

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

        var sp = GoodsManager.Instance.GetGoods(GoodsType.SP);
        if (sp != null)
            sp.Increase((uint)amount);
    }
}
