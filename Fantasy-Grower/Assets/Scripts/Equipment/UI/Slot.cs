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

    protected void Start()
    {
        MyImage = GetComponent<Image>();
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
        MyImage.color = EquipmentManager.Instance.GetColor(ID);
        WeaponNameText.text = GetWeapon().weaponName;
        UpdateUpgrade();
        if (GetWeapon().isUnlock)
        {
            WeaponIconWall.SetActive(false);
            WeaponIcon.color = Color.white;
        }
        else
        {
            WeaponIconWall.SetActive(true);
        }
    }

    private void UpdateUpgrade()
    {
        if (GetWeapon().weaponLevel > 0)
        {
            WeaponUpgradeText.text = $"+{GetWeapon().weaponLevel}";
        }
        else
            WeaponUpgradeText.gameObject.SetActive(false);

        for (int i = 0; i < GetWeapon().weaponAwakeLevel; i++)
        {
            AwakeImages[i].gameObject.SetActive(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!WeaponIconWall.activeSelf)
            EquipmentManager.Instance.OpenItemInfoPage(this);
    }
}
