using TMPro;
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
    private TMP_Text slotLabel;

    [SerializeField]
    private Image background;

    private int slotIndex;
    private SkillTreePanel panel;

    private static readonly Color ColorFilled = Color.cyan;
    private static readonly Color ColorEmpty = Color.gray;

    public SkillCategory Category => category;

    public void Initialize(int index, SkillTreePanel ownerPanel)
    {
        slotIndex = index;
        panel = ownerPanel;
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    public void Refresh(SkillData skill)
    {
        if (slotLabel != null)
            slotLabel.text = skill != null ? skill.SkillName : "(비어있음)";
        if (background != null)
            background.color = skill != null ? ColorFilled : ColorEmpty;
    }

    private void OnClicked() => panel?.OnEquipSlotClicked(slotIndex, category);
}
