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
    public string weaponID;

    [Header("UI속성")]
    public SlotType _SlotType;
    public GameObject WeaponIconWall;
    public Image WeaponIcon;
    public Text WeaponCountText;

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

    SO_Weapon GetWeapon() => EquipmentManager.Instance.GetWeapon(weaponID);

    public void RefreshIcon()
    {
        GetComponent<Image>().color = EquipmentManager.weaponLevelColor[(int)GetWeapon().Rate];
        WeaponIcon.sprite = GetWeapon().WeaponIcon;
        if (GetWeapon().isUnlock)
        {
            WeaponIconWall.SetActive(false);
            WeaponIcon.color = Color.white;
            // 무기 개수 text 필요
        }
        else
        {
            WeaponIconWall.SetActive(true);
        }
    }
}
