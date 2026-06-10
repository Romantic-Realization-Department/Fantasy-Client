using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 트리 단일 노드를 표시하는 버튼. SkillTreePanel.BuildTree()에서 초기화된다.
/// </summary>
[RequireComponent(typeof(Button))]
public class SkillNodeButton : MonoBehaviour
{
    [SerializeField]
    private TMP_Text skillNameText;

    [SerializeField]
    private TMP_Text spCostText;

    [SerializeField]
    private Image background;

    private SkillNodeData node;
    private SkillTreePanel panel;

    private static readonly Color ColorUnlocked = Color.green;
    private static readonly Color ColorCanUnlock = Color.yellow;
    private static readonly Color ColorSelected = Color.blue;
    private static readonly Color ColorLocked = Color.gray;

    public void Initialize(SkillNodeData nodeData, SkillTreePanel ownerPanel)
    {
        node = nodeData;
        panel = ownerPanel;

        bool isPassive = nodeData.Skill is PassiveSkillData;
        string suffix = isPassive ? " (패시브)" : string.Empty;

        if (skillNameText != null)
            skillNameText.text =
                nodeData.Skill != null ? $"{nodeData.Skill.SkillName}{suffix}" : "(없음)"; // + 연산 삭제

        if (spCostText != null)
            spCostText.text = nodeData.Skill != null ? $"SP: {nodeData.Skill.SPCost}" : "SP: 0";

        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    public void Refresh(bool isUnlocked, bool canUnlock, bool isSelected)
    {
        if (background == null)
            return;

        if (isSelected)
            background.color = ColorSelected;
        else if (isUnlocked)
            background.color = ColorUnlocked;
        else if (canUnlock)
            background.color = ColorCanUnlock;
        else
            background.color = ColorLocked;
    }

    private void OnClicked() => panel?.OnNodeClicked(node);
}
