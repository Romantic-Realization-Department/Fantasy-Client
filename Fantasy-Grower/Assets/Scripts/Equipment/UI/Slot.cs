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
    private Text WeaponCountText;

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

    public void OnPointerClick(PointerEventData eventData)
    {
        EquipmentManager.Instance.OpenItemInfoPage(this);
    }
}
