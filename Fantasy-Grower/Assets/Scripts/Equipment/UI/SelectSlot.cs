using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("무기")]
    [Tooltip("ID는 S2보다는 큰 값으로 해주세요"), SerializeField]
    private WeaponID ID;

    SO_Weapon _Weapon;

    [Header("UI속성")]
    [SerializeField]
    private Image WeaponIcon;

    [SerializeField]
    private Image BGImage;

    private void Awake()
    {
        if (ID <= WeaponID.S2)
        {
            return;
        }
        _Weapon = EquipmentManager.Instance.GetWeapon(ID);
        WeaponIcon.sprite = EquipmentManager.Instance.GetIcon(ID);
        BGImage.color = EquipmentManager.Instance.GetColor(ID);
    }

    private void OnEnable()
    {
        if (!_Weapon.isUnlock || !EquipmentManager.Instance.CanSynth(ID))
            gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        EquipmentManager.Instance.SaveSynthWeapon(ID);
    }
}
