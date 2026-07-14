using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentUIManager : MonoBehaviour
{
    public static EquipmentUIManager Instance { get; private set; }

    private readonly char[] costTextBuffer = new char[128];

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    [Header("장비 탭")]
    [field: SerializeField]
    public GameObject WeaponInfoObject;

    [field: SerializeField]
    public Image WeaponIconImage { get; private set; }

    [field: SerializeField]
    public Image WeaponBGImage { get; private set; }

    [field: SerializeField]
    public TMP_Text WeaponLevelText { get; private set; }

    [field: SerializeField]
    public TMP_Text EquipInfoText { get; private set; }

    [field: SerializeField]
    public TMP_Text GetInfoText { get; private set; }

    [field: SerializeField]
    public GameObject[] AwakeObject { get; private set; }

    [field: SerializeField]
    public GameObject EquipButtonObject { get; private set; }

    [field: SerializeField]
    public GameObject UnequipButtonObject { get; private set; }

    [SerializeField]
    private Slot[] slots;

    [Header("강화")]
    [field: SerializeField]
    public Image UpgradeWeaponIconImage { get; private set; }

    [field: SerializeField]
    public Image UpgradeWeaponBGImage { get; private set; }

    [field: SerializeField]
    public TMP_Text UpgradeWeaponLevelText { get; private set; }

    [field: SerializeField]
    public TMP_Text UpgradeWeaponLevelUpText { get; private set; }

    [field: SerializeField]
    public TMP_Text UpgradeEquipInfoText { get; private set; }

    [field: SerializeField]
    public TMP_Text UpgradeGetInfoText { get; private set; }

    [field: SerializeField]
    public TMP_Text UpgradeCostText { get; private set; }

    [field: SerializeField]
    public GameObject[] UpgradeAwakeObject { get; private set; }

    [Header("대장간 변수")]
    [field: SerializeField]
    public GameObject SmithyTab { get; private set; }

    [Header("합성")]
    [field: SerializeField]
    public AnvilSlot[] SynthSlots { get; private set; }

    [Header("각성")]
    [field: SerializeField]
    public AnvilSlot[] AwakeSlots { get; private set; }

    [field: SerializeField]
    public TMP_Text AwakeMithrilCostText { get; private set; }

    public void SettingItemInfoUI(Color color, Sprite sprite, SO_Weapon currentWeapon)
    {
        WeaponIconImage.sprite = sprite;
        WeaponBGImage.color = color;
        WeaponLevelText.SetText("+{0}", currentWeapon.weaponLevel);
        WeaponLevelText.gameObject.SetActive(currentWeapon.weaponLevel > 0);
        for (int i = 0; i < AwakeObject.Length; i++)
        {
            AwakeObject[i].SetActive(currentWeapon.weaponAwakeLevel > i);
        }
        EquipInfoText.SetText("공격력 +{0}", currentWeapon.EquipDamage());
        GetInfoText.SetText("공격력 +{0}", currentWeapon.DefaultDamage());
        WeaponInfoObject.SetActive(true);
        Debug.Log(WeaponInfoObject.activeSelf);
    }

    public void SettingUpgradeItemInfoUI(Color color, Sprite sprite, SO_Weapon currentWeapon)
    {
        UpgradeWeaponIconImage.sprite = sprite;
        UpgradeWeaponBGImage.color = color;
        UpgradeWeaponLevelText.SetText("+{0}", currentWeapon.weaponLevel);
        UpgradeWeaponLevelText.gameObject.SetActive(currentWeapon.weaponLevel > 0);
        for (int i = 0; i < UpgradeAwakeObject.Length; i++)
        {
            UpgradeAwakeObject[i].SetActive(currentWeapon.weaponAwakeLevel > i);
        }
        if (currentWeapon.weaponLevel < EquipmentManager.Instance.maxUpgradeLevel)
        {
            int currentEquipDamage = currentWeapon.EquipDamage();
            int currentDefaultDamage = currentWeapon.DefaultDamage();
            currentWeapon.weaponLevel++;
            try
            {
                int upgradeEquipDamage = currentWeapon.EquipDamage();
                int upgradeDefaultDamage = currentWeapon.DefaultDamage();
                UpgradeEquipInfoText.SetText(
                    "공격력 +{0}<color=green>(+{1})</color>",
                    currentEquipDamage,
                    upgradeEquipDamage - currentEquipDamage
                );
                UpgradeGetInfoText.SetText(
                    "공격력 +{0}<color=green>(+{1})</color>",
                    currentDefaultDamage,
                    upgradeDefaultDamage - currentDefaultDamage
                );
                UpgradeWeaponLevelUpText.gameObject.SetActive(true);
            }
            finally
            {
                currentWeapon.weaponLevel--;
            }
        }
        else
        {
            UpgradeEquipInfoText.SetText("공격력 +{0}", currentWeapon.EquipDamage());
            UpgradeGetInfoText.SetText("공격력 +{0}", currentWeapon.DefaultDamage());
            UpgradeWeaponLevelUpText.gameObject.SetActive(false);
        }

        SettingUpgradeCostUI(currentWeapon);
    }

    public void SettingUpgradeCostUI(SO_Weapon currentWeapon)
    {
        if (UpgradeCostText == null)
            return;

        if (
            currentWeapon == null
            || EquipmentManager.Instance == null
            || currentWeapon.weaponLevel >= EquipmentManager.Instance.maxUpgradeLevel
        )
        {
            UpgradeCostText.SetText("최대 강화");
            return;
        }

        RewardDisplayItem upgradeCost = RewardDisplayItemFactory.Goods(
            GoodsType.UpgradeScroll,
            (uint)EquipmentManager.Instance.useScrollAmount
        );
        SetRewardDisplayItemText(UpgradeCostText, upgradeCost);
    }

    public void SettingAwakeCostUI(SO_Weapon currentWeapon, uint mithrilAmount)
    {
        if (currentWeapon == null || EquipmentManager.Instance == null)
        {
            ClearAwakeCostUI();
            return;
        }

        if (currentWeapon.weaponAwakeLevel >= EquipmentManager.Instance.maxAwakeLevel)
        {
            if (AwakeMithrilCostText != null)
                AwakeMithrilCostText.SetText("최대 각성");

            return;
        }

        if (AwakeMithrilCostText != null)
        {
            RewardDisplayItem mithrilCost = RewardDisplayItemFactory.Goods(
                GoodsType.Mithril,
                mithrilAmount
            );
            SetRewardDisplayItemText(AwakeMithrilCostText, mithrilCost);
        }
    }

    public void ClearAwakeCostUI()
    {
        if (AwakeMithrilCostText != null)
            AwakeMithrilCostText.SetText("");
    }

    public void SettingEquipStateUI(bool isEquipped)
    {
        if (EquipButtonObject != null)
            EquipButtonObject.SetActive(!isEquipped);

        if (UnequipButtonObject != null)
            UnequipButtonObject.SetActive(isEquipped);
    }

    private void SetRewardDisplayItemText(TMP_Text text, RewardDisplayItem item)
    {
        int offset = 0;
        Span<char> charSpan = costTextBuffer;
        if (
            !charSpan.TryAppend(ref offset, "<sprite name=\"".AsSpan())
            || !charSpan.TryAppend(ref offset, item.IconName.AsSpan())
            || !charSpan.TryAppend(ref offset, "\"> : ".AsSpan())
            || !charSpan.TryAppend(ref offset, item.Amount, "N0")
        )
        {
            Debug.LogWarning(
                "[EquipmentUIManager] 강화 비용 텍스트 버퍼 범위를 초과했습니다.",
                this
            );
            text.SetText("");
            return;
        }

        text.SetCharArray(costTextBuffer, 0, offset);
    }

    public void RefreshSlotUI()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].RefreshIcon();
    }

    public void Equip() => EquipmentManager.Instance.Equip(); //버튼 추가 형

    public void Unequip() => EquipmentManager.Instance.Unequip(); //버튼 추가 형

    public void UpgradeWeapon() => EquipmentManager.Instance.UpgradeWeapon(); //버튼 추가 형

    public void Synthesis() => EquipmentManager.Instance.Synthesis(); //버튼 추가 형

    public void Awakening() => EquipmentManager.Instance.Awakening(); //버튼 추가 형
}
