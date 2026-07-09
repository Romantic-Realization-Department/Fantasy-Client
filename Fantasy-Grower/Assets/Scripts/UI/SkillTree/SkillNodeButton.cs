using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킬 트리 단일 노드를 표시하는 버튼. SkillTreePanel.BuildTree()에서 초기화된다.
/// </summary>
[RequireComponent(typeof(Button))]
public class SkillNodeButton : MonoBehaviour
{
    [SerializeField]
    private Image background;

    [Header("상태 색상")]
    [SerializeField]
    private Color equippedColor = new(0.45f, 1f, 0.95f, 1f);

    [SerializeField]
    private Color unlockedColor = Color.white;

    [SerializeField]
    private Color lockedColor = new(0.25f, 0.25f, 0.25f, 1f);

    private SkillNodeData node;
    private SkillTreePanel panel;

    public void Initialize(SkillNodeData nodeData, SkillTreePanel ownerPanel)
    {
        node = nodeData;
        panel = ownerPanel;

        ApplySkillIcon();
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    private void ApplySkillIcon()
    {
        if (background == null || node == null || node.Skill == null)
            return;

        Sprite icon = node.Skill.SkillIcon;
        background.sprite = icon;
        background.preserveAspect = true;
        background.gameObject.SetActive(icon != null);
    }

    public void Refresh(bool isUnlocked, bool isEquipped)
    {
        if (background == null)
            return;

        background.color = GetStateColor(isUnlocked, isEquipped);
    }

    private Color GetStateColor(bool isUnlocked, bool isEquipped)
    {
        if (isEquipped)
            return equippedColor;
        if (isUnlocked)
            return unlockedColor;

        return lockedColor;
    }

    private void OnClicked()
    {
        if (panel != null)
            panel.OnNodeClicked(node);
    }
}
