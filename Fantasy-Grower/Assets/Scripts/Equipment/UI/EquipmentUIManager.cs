using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentUIManager : MonoBehaviour
{
    public static EquipmentUIManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance != null)
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

    public void SettingItemInfoUI(Color color, Sprite sprite, SO_Weapon currentWeapon)
    {
        WeaponIconImage.sprite = sprite;
        WeaponBGImage.color = color;
        WeaponLevelText.text = "+" + currentWeapon.weaponLevel;
        WeaponLevelText.gameObject.SetActive(currentWeapon.weaponLevel > 0);
        for (int i = 0; i < AwakeObject.Length; i++)
        {
            AwakeObject[i].SetActive(currentWeapon.weaponAwakeLevel > i);
        }
        EquipInfoText.text = "공격력 +" + currentWeapon.EquipDamage();
        GetInfoText.text = "공격력 +" + currentWeapon.DefaultDamage();
        WeaponInfoObject.SetActive(true);
        Debug.Log(WeaponInfoObject.activeSelf);
    }

    public void SettingUpgradeItemInfoUI(Color color, Sprite sprite, SO_Weapon currentWeapon)
    {
        UpgradeWeaponIconImage.sprite = sprite;
        UpgradeWeaponBGImage.color = color;
        UpgradeWeaponLevelText.text = "+" + currentWeapon.weaponLevel;
        UpgradeWeaponLevelText.gameObject.SetActive(currentWeapon.weaponLevel > 0);
        for (int i = 0; i < UpgradeAwakeObject.Length; i++)
        {
            UpgradeAwakeObject[i].SetActive(currentWeapon.weaponAwakeLevel > i);
        }
        if (currentWeapon.weaponLevel < EquipmentManager.Instance.maxUpgradeLevel)
        {
            float currentED = currentWeapon.EquipDamage();
            float currentDD = currentWeapon.DefaultDamage();
            currentWeapon.weaponLevel++;
            try
            {
                float upgradeED = currentWeapon.EquipDamage();
                float upgradeDD = currentWeapon.DefaultDamage();
                UpgradeEquipInfoText.text =
                    "공격력 +"
                    + currentWeapon.EquipDamage()
                    + $"<color=green>(+{upgradeED - currentWeapon.EquipDamage()})</color>";
                UpgradeGetInfoText.text =
                    "공격력 +"
                    + currentWeapon.DefaultDamage()
                    + $"<color=green>(+{upgradeDD - currentWeapon.DefaultDamage()})</color>";
                UpgradeWeaponLevelUpText.gameObject.SetActive(true);
            }
            finally
            {
                currentWeapon.weaponLevel--;
            }
        }
        else
        {
            UpgradeEquipInfoText.text = "공격력 +" + currentWeapon.EquipDamage();
            UpgradeGetInfoText.text = "공격력 +" + currentWeapon.DefaultDamage();
            UpgradeWeaponLevelUpText.gameObject.SetActive(false);
        }
    }

    public void RefreshSlotUI()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].RefreshIcon();
    }

    public void Equip() => EquipmentManager.Instance.Equip(); //버튼 추가 형

    public void UpgradeWeapon() => EquipmentManager.Instance.UpgradeWeapon(); //버튼 추가 형

    public void Synthesis() => EquipmentManager.Instance.Synthesis(); //버튼 추가 형

    public void Awakening() => EquipmentManager.Instance.Awakening(); //버튼 추가 형
}
