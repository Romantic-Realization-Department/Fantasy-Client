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

    [SerializeField]
    private Text WeaponNameText;

    protected void Start()
    {
        WeaponIcon.sprite = getIcon();
        RefreshIcon();
    }

    SO_Weapon GetWeapon() => EquipmentManager.Instance.GetWeapon(ID);

    Sprite getIcon() => EquipmentManager.Instance.GetIcon(ID);

    public void RefreshIcon()
    {
        GetComponent<Image>().color = EquipmentManager.Instance.GetColor(ID);
        WeaponNameText.text = GetWeapon().weaponName;
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

    public void OnPointerClick(PointerEventData eventData)
    {
        EquipmentManager.Instance.OpenItemInfoPage(this);
    }
}
