using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 장착 슬롯 UI. 액티브/패시브 카테고리를 구분하여 동작한다.
/// 클릭 시 SkillTreePanel에 카테고리와 함께 장착 요청을 전달한다.
/// </summary>
[RequireComponent(typeof(Button))]
public class SkillEquipSlotUI : MonoBehaviour
{
    [SerializeField]
    private SkillCategory category = SkillCategory.Active;

    [SerializeField]
    private Image skillIconImage;

    [Header("장착 대기 표시")]
    [SerializeField]
    private Graphic highlightTarget;

    [SerializeField]
    private Color highlightedColor = new(0.55f, 0.8f, 1f, 1f);

    private int slotIndex;
    private SkillTreePanel panel;
    private Button button;
    private Graphic resolvedHighlightTarget;
    private Color defaultHighlightColor;
    private bool hasDefaultHighlightColor;

    public SkillCategory Category => category;

    private void Awake()
    {
        button = GetComponent<Button>();
        ResolveHighlightTarget();
    }

    public void Initialize(int index, SkillTreePanel ownerPanel, SkillCategory slotCategory)
    {
        slotIndex = index;
        panel = ownerPanel;
        category = slotCategory;

        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnClicked);
    }

    public void Refresh(SkillData skill, bool isHighlighted)
    {
        RefreshHighlight(isHighlighted);

        if (skillIconImage == null)
            return;

        Sprite icon = skill != null ? skill.SkillIcon : null;
        skillIconImage.sprite = icon;
        skillIconImage.preserveAspect = true;
        skillIconImage.gameObject.SetActive(icon != null);
    }

    private void RefreshHighlight(bool isHighlighted)
    {
        ResolveHighlightTarget();

        if (resolvedHighlightTarget == null || !hasDefaultHighlightColor)
            return;

        resolvedHighlightTarget.color = isHighlighted ? highlightedColor : defaultHighlightColor;
    }

    private void ResolveHighlightTarget()
    {
        if (resolvedHighlightTarget != null && hasDefaultHighlightColor)
            return;

        if (button == null)
            button = GetComponent<Button>();

        resolvedHighlightTarget = highlightTarget;
        if (resolvedHighlightTarget == null && button != null)
            resolvedHighlightTarget = button.targetGraphic;
        if (resolvedHighlightTarget == null)
            resolvedHighlightTarget = GetComponent<Graphic>();

        if (resolvedHighlightTarget == null)
            return;

        // 장착 대기 상태가 끝나면 원래 슬롯 색상으로 되돌리기 위해 최초 색상을 보관합니다.
        defaultHighlightColor = resolvedHighlightTarget.color;
        hasDefaultHighlightColor = true;
    }

    private void OnClicked()
    {
        if (panel != null)
            panel.OnEquipSlotClicked(slotIndex, category);
    }
}
