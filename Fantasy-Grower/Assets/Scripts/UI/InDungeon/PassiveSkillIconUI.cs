using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class PassiveSkillIconUI : MonoBehaviour
{
    [SerializeField, Min(0)]
    private int slotIndex;

    [Header("Display")]
    [SerializeField]
    private Image iconImage;

    private GameManager boundGameManager;
    private SkillTreeComponent skillTreeComponent;
    private PassiveSkillData currentSkill;

    private void OnEnable()
    {
        BindGameManager();
        BindCurrentPlayer();
        Refresh(true);
    }

    private void Update()
    {
        Refresh(false);
    }

    private void OnDisable()
    {
        UnbindGameManager();
    }

    public void Initialize(int index, SkillTreeComponent component)
    {
        slotIndex = Mathf.Max(0, index);
        skillTreeComponent = component;
        currentSkill = null;
        Refresh(true);
    }

    private void BindGameManager()
    {
        GameManager gameManager = GameManager.InstanceOrNull;
        if (boundGameManager == gameManager)
            return;

        UnbindGameManager();

        boundGameManager = gameManager;
        if (boundGameManager != null)
            boundGameManager.OnPlayerChanged += HandlePlayerChanged;
    }

    private void UnbindGameManager()
    {
        if (boundGameManager != null)
            boundGameManager.OnPlayerChanged -= HandlePlayerChanged;

        boundGameManager = null;
    }

    private void BindCurrentPlayer()
    {
        Entity player = boundGameManager != null ? boundGameManager.GetPlayer() : null;
        BindPlayer(player);
    }

    private void HandlePlayerChanged(Entity player)
    {
        BindPlayer(player);
        Refresh(true);
    }

    private void BindPlayer(Entity player)
    {
        skillTreeComponent = null;
        currentSkill = null;

        if (player != null)
            player.TryGetComponent(out skillTreeComponent);
    }

    private void Refresh(bool force)
    {
        if (skillTreeComponent == null)
            BindCurrentPlayer();

        PassiveSkillData skill =
            skillTreeComponent != null ? skillTreeComponent.GetEquippedPassive(slotIndex) : null;
        if (!force && skill == currentSkill)
            return;

        currentSkill = skill;
        RefreshSkillView(skill);
    }

    private void RefreshSkillView(PassiveSkillData skill)
    {
        bool hasSkill = skill != null;

        if (iconImage != null)
        {
            iconImage.sprite = hasSkill ? skill.SkillIcon : null;
            iconImage.preserveAspect = true;
            iconImage.enabled = hasSkill && skill.SkillIcon != null;
        }
    }

    private void OnValidate()
    {
        slotIndex = Mathf.Max(0, slotIndex);
    }
}
