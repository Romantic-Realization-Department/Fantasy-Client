using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum SlotType
{
    Inventory,
    Synthesis,
    Enchent,
}

public class Slot : MonoBehaviour, IPointerClickHandler
{
    [Header("아이템 속성")]
    public WeaponID ID;

    [Header("UI속성")]
    [SerializeField]
    private SlotType _SlotType;

    [SerializeField]
    private GameObject WeaponIconWall;

    [SerializeField]
    private Image WeaponIcon;

    private Image MyImage;

    [SerializeField]
    private Image[] AwakeImages;

    [SerializeField]
    private TMP_Text WeaponNameText;

    [SerializeField]
    private TMP_Text WeaponUpgradeText;

    protected void Awake()
    {
        MyImage = GetComponent<Image>();
    }

    protected void Start()
    {
        if (WeaponIcon != null)
            WeaponIcon.sprite = getIcon();

        RefreshIcon();
    }

    protected void OnEnable()
    {
        if (EquipmentManager.Instance != null && MyImage != null)
            RefreshIcon();
    }

    SO_Weapon GetWeapon() => EquipmentManager.Instance.GetWeapon(ID);

    Sprite getIcon() => EquipmentManager.Instance.GetIcon(ID);

    public void RefreshIcon()
    {
        if (EquipmentManager.Instance == null || MyImage == null)
            return;

        SO_Weapon weapon = GetWeapon();
        if (weapon == null)
            return;

        MyImage.color = EquipmentManager.Instance.GetColor(ID);
        if (WeaponNameText != null)
            WeaponNameText.text = weapon.weaponName;

        UpdateUpgrade(weapon);
        if (weapon.isUnlock)
        {
            if (WeaponIconWall != null)
                WeaponIconWall.SetActive(false);
            if (WeaponIcon != null)
                WeaponIcon.color = Color.white;
        }
        else
        {
            if (WeaponIconWall != null)
                WeaponIconWall.SetActive(true);
        }
    }

    private void UpdateUpgrade(SO_Weapon weapon)
    {
        if (WeaponUpgradeText != null)
        {
            bool hasUpgrade = weapon.weaponLevel > 0;
            WeaponUpgradeText.gameObject.SetActive(hasUpgrade);
            if (hasUpgrade)
                WeaponUpgradeText.text = $"+{weapon.weaponLevel}";
        }

        for (int i = 0; i < AwakeImages.Length; i++)
        {
            if (AwakeImages[i] != null)
                AwakeImages[i].gameObject.SetActive(i < weapon.weaponAwakeLevel);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (WeaponIconWall != null && !WeaponIconWall.activeSelf)
            EquipmentManager.Instance.OpenItemInfoPage(this);
    }
}
