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
    private Text WeaponNameText;

    [SerializeField]
    private Text WeaponUpgradeText;

    protected void Start()
    {
        WeaponIcon.sprite = getIcon();
        RefreshIcon();
    }

    protected void OnEnable()
    {
        RefreshIcon();
    }

    SO_Weapon GetWeapon() => EquipmentManager.Instance.GetWeapon(ID);

    Sprite getIcon() => EquipmentManager.Instance.GetIcon(ID);

    private void RefreshIcon()
    {
        MyImage.color = EquipmentManager.Instance.GetColor(ID);
        WeaponNameText.text = GetWeapon().weaponName;
        if (GetWeapon().isUnlock)
        {
            WeaponIconWall.SetActive(false);
            WeaponIcon.color = Color.white;
            UpdateUpgrade();
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
        EquipmentManager.Instance.OpenItemInfoPage(this);
    }
}
