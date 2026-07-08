using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 모바일 던전 UI에서 액티브 스킬 슬롯 하나를 실행하는 버튼입니다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public sealed class ActiveSkillButtonUI : MonoBehaviour
{
    [SerializeField, Min(0)]
    private int slotIndex;

    [Header("표시")]
    [SerializeField]
    private Image iconImage;

    [SerializeField]
    private Image cooldownFillImage;

    [SerializeField]
    private TMP_Text cooldownText;

    [SerializeField]
    private GameObject emptySlotObject;

    private ActiveSkillExecutor cachedExecutor;

    private Button button;
    private ActiveSkillData currentSkill;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(OnClicked);
        ResolveExecutor();
        Refresh(true);
    }

    private void Update()
    {
        Refresh(false);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClicked);
    }

    public void Initialize(int index, ActiveSkillExecutor skillExecutor)
    {
        slotIndex = Mathf.Max(0, index);
        cachedExecutor = skillExecutor;
        Refresh(true);
    }

    private void OnClicked()
    {
        ActiveSkillExecutor executor = ResolveExecutor();
        if (executor != null)
            executor.TryUseSkill(slotIndex);

        Refresh(true);
    }

    private ActiveSkillExecutor ResolveExecutor()
    {
        if (cachedExecutor == null)
            cachedExecutor = FindAnyObjectByType<ActiveSkillExecutor>();

        return cachedExecutor;
    }

    private void Refresh(bool force)
    {
        ActiveSkillExecutor executor = ResolveExecutor();
        ActiveSkillData skill = executor != null ? executor.GetEquippedSkill(slotIndex) : null;
        if (force || skill != currentSkill)
        {
            currentSkill = skill;
            RefreshSkillView(skill);
        }

        RefreshCooldown(executor, skill);
    }

    private void RefreshSkillView(ActiveSkillData skill)
    {
        bool hasSkill = skill != null;

        if (iconImage != null)
        {
            iconImage.sprite = hasSkill ? skill.SkillIcon : null;
            iconImage.enabled = hasSkill && skill.SkillIcon != null;
        }

        if (emptySlotObject != null)
            emptySlotObject.SetActive(!hasSkill);
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
                cooldownText.text = Mathf.CeilToInt(cooldownRemaining).ToString();
        }
    }

    private void OnValidate()
    {
        slotIndex = Mathf.Max(0, slotIndex);
    }
}
