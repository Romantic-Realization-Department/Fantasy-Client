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
        TryInitialize();
    }

    private void OnEnable()
    {
        if (!TryInitialize())
        {
            if (CantUseWall != null)
                CantUseWall.SetActive(true);
            return;
        }

        if (CantUseWall != null)
            CantUseWall.SetActive(!(_Weapon.isUnlock && EquipmentManager.Instance.CanUse(ID)));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (CantUseWall != null && !CantUseWall.activeSelf)
            EquipmentManager.Instance.SaveSelectWeapon(ID, selectSlotType);
    }

    private bool TryInitialize()
    {
        if (_Weapon != null)
            return true;

        if (selectSlotType == SelectSlotType.Synth && ID <= WeaponID.S2)
            return false;

        if (EquipmentManager.Instance == null)
            return false;

        _Weapon = EquipmentManager.Instance.GetWeapon(ID);
        if (_Weapon == null)
            return false;

        if (WeaponIcon != null)
            WeaponIcon.sprite = EquipmentManager.Instance.GetIcon(ID);
        if (BGImage != null)
            BGImage.color = EquipmentManager.Instance.GetColor(ID);

        return true;
    }
}
