using UnityEngine;
using UnityEngine.UI;

public enum SlotType
{
    Inventory,
    Synthesis,
    Enchent,
}

public class Slot : MonoBehaviour
{
    [Header("아이템 속성")]
    public WeaponType weaponType;
    public WeaponLevel weaponLevel;

    [Header("UI속성")]
    public SlotType slotType;
    public GameObject weaponIconWall;
    public Image weaponIcon;
    public Text weaponCountText;

    protected void Start()
    {
        RefreshIcon();
        if (TryGetComponent<Button>(out Button button))
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    protected void OnButtonClick()
    {
        EquipmentManager.Instance.OpenItemInfoPage(this);
    }

    public void GetItem() =>
        EquipmentManager.Instance.GetWeapon(weaponType, weaponLevel).weaponCount++;

    public void RefreshIcon()
    {
        GetComponent<Image>().color = EquipmentManager.weaponLevelColor[(int)weaponLevel];
        weaponIcon.sprite = EquipmentManager.Instance.GetWeapon(weaponType, weaponLevel).weaponIcon;
        if (EquipmentManager.Instance.GetWeapon(weaponType, weaponLevel).isUnlock)
        {
            weaponIconWall.SetActive(false);
            weaponIcon.color = Color.white;
            // 무기 개수 text 필요
        }
        else
        {
            weaponIconWall.SetActive(true);
        }
    }
}
