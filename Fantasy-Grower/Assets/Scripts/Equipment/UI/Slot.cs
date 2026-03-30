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
    [Header("酒捞袍 加己")]
    public WeaponType weaponType;
    public WeaponLevel weaponLevel;

    [Header("UI加己")]
    public Image weaponIcon;
    public SlotType slotType;
    public Text weaponCountText;

    protected void Awake()
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
        if (EquipmentManager.Instance.GetWeapon(weaponType, weaponLevel).weaponCount > 0)
        {
            weaponIcon.color = Color.white;
        }
        else
        {
            weaponIcon.color = new Color(0, 0, 0, .3f);
        }
    }
}
