using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("무기")]
    [Tooltip("ID는 S2보다는 큰 값으로 해주세요")]
    public WeaponID ID;
    SO_Weapon _Weapon;

    [Header("UI속성")]
    public Image WeaponIcon;
    public Image BGImage;

    private void Awake()
    {
        if (ID <= WeaponID.S2)
            Debug.LogError("잘못된 ID값 입니다");
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
        Debug.Log(ID);
        Debug.Log(_Weapon.weaponCount);
        EquipmentManager.Instance.SaveSynthWeapon(ID);
    }
}
