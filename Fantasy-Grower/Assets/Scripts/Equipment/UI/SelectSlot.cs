using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum SelectSlotType
{
    Synth,
    Awake,
}

public class SelectSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("무기")]
    [Tooltip("합성 이라면 ID는 S2보다는 큰 값으로 해주세요"), SerializeField]
    private WeaponID ID;

    SO_Weapon _Weapon;

    [Header("UI속성")]
    [SerializeField]
    private Image WeaponIcon;

    [SerializeField]
    private Image BGImage;

    [SerializeField]
    private GameObject CantUseWall;

    [SerializeField]
    private SelectSlotType selectSlotType;

    private void Awake()
    {
        if (selectSlotType == SelectSlotType.Synth && ID <= WeaponID.S2)
        {
            return;
        }
        _Weapon = EquipmentManager.Instance.GetWeapon(ID);
        WeaponIcon.sprite = EquipmentManager.Instance.GetIcon(ID);
        BGImage.color = EquipmentManager.Instance.GetColor(ID);
    }

    private void OnEnable()
    {
        CantUseWall.SetActive(!(_Weapon.isUnlock && EquipmentManager.Instance.CanUse(ID)));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CantUseWall.activeSelf)
            EquipmentManager.Instance.SaveSelectWeapon(ID, selectSlotType);
    }
}
