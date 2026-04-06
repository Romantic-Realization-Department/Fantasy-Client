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
    public WeaponType _WeaponType;
    public WeaponLevel _WeaponLevel;

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

    public void GetItem() =>
        EquipmentManager.Instance.GetWeapon(_WeaponType, _WeaponLevel).weaponCount++;

    public void RefreshIcon()
    {
        GetComponent<Image>().color = EquipmentManager.weaponLevelColor[(int)_WeaponLevel];
        WeaponIcon.sprite = EquipmentManager
            .Instance.GetWeapon(_WeaponType, _WeaponLevel)
            .WeaponIcon;
        if (EquipmentManager.Instance.GetWeapon(_WeaponType, _WeaponLevel).isUnlock)
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
