using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public sealed class ActiveSkillButtonUI : MonoBehaviour
{
    [SerializeField, Min(0)]
    private int slotIndex;

    [Header("Display")]
    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private Image cooldownFillImage;

    [SerializeField]
    private TMP_Text cooldownText;

    private GameManager boundGameManager;
    private ActiveSkillExecutor cachedExecutor;

    private Button button;
    private ActiveSkillData currentSkill;
    private int lastCooldownSecond = -1;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(OnClicked);
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
        button.onClick.RemoveListener(OnClicked);
        UnbindGameManager();
    }

    public void Initialize(int index, ActiveSkillExecutor skillExecutor)
    {
        slotIndex = Mathf.Max(0, index);
        cachedExecutor = skillExecutor;
        currentSkill = null;
        Refresh(true);
    }

    private void OnClicked()
    {
        ActiveSkillExecutor executor = GetExecutor();
        if (executor != null)
            executor.TryUseSkill(slotIndex);

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
        cachedExecutor = null;
        currentSkill = null;
        lastCooldownSecond = -1;

        if (player != null)
            player.TryGetComponent(out cachedExecutor);
    }

    private ActiveSkillExecutor GetExecutor()
    {
        if (cachedExecutor == null)
            BindCurrentPlayer();

        return cachedExecutor;
    }

    private void Refresh(bool force)
    {
        ActiveSkillExecutor executor = GetExecutor();
        ActiveSkillData skill = executor != null ? executor.GetEquippedSkill(slotIndex) : null;
        if (force || skill != currentSkill)
        {
            currentSkill = skill;
            RefreshSkillView(skill);
            lastCooldownSecond = -1;
        }

        RefreshCooldown(executor, skill);
    }

    private void RefreshSkillView(ActiveSkillData skill)
    {
        bool hasSkill = skill != null;

        if (iconImage != null)
        {
            iconImage.sprite = hasSkill ? skill.SkillIcon : null;
            iconImage.preserveAspect = true;
            iconImage.enabled = hasSkill && skill.SkillIcon != null;
        }
    }

    private void RefreshCooldown(ActiveSkillExecutor executor, ActiveSkillData skill)
    {
        float cooldownRemaining =
            executor != null && skill != null ? executor.GetCooldownRemaining(slotIndex) : 0f;
        bool isCooldown = cooldownRemaining > 0f;
        bool isOncePerDungeonUsed =
            executor != null && skill != null && executor.IsOncePerDungeonSkillUsed(slotIndex);

        button.interactable = skill != null && !isCooldown && !isOncePerDungeonUsed;

        if (cooldownFillImage != null)
        {
            cooldownFillImage.fillAmount =
                executor != null && skill != null ? executor.GetCooldownRatio(slotIndex) : 0f;
            cooldownFillImage.gameObject.SetActive(isCooldown);
        }

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(isCooldown);
            if (isCooldown)
            {
                int cooldownSecond = Mathf.CeilToInt(cooldownRemaining);
                if (cooldownSecond != lastCooldownSecond)
                {
                    lastCooldownSecond = cooldownSecond;
                    cooldownText.text = cooldownSecond.ToString();
                }
            }
            else
            {
                lastCooldownSecond = -1;
            }
        }
    }

    private void OnValidate()
    {
        slotIndex = Mathf.Max(0, slotIndex);
    }
}
