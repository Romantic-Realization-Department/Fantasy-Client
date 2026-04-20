using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SynthSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("SlotUI속성")]
    [SerializeField]
    private Image SlotImage;

    [SerializeField]
    private Image BGImage;

    [SerializeField]
    private Text WeaponCountText; //TMP로 바뀔 예정

    [Header("선택 UI 속성")]
    [SerializeField]
    private GameObject SelectUI;

    [SerializeField]
    private GameObject[] SelectSlots; //전용 클래스 생성으로 변환

    [Header("재료와 결과 분리")]
    public bool isSelectSlot;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isSelectSlot)
        {
            ShowSelectUI();
        }
    }

    private void ShowSelectUI()
    {
        for (int i = 0; i < SelectSlots.Length; i++)
            SelectSlots[i].SetActive(true);
        SelectUI.SetActive(true);
    }

    public void SwapImage(SO_Weapon weapon, Sprite icon, Color _color)
    {
        SlotImage.sprite = icon;
        BGImage.color = _color;
        WeaponCountText.text = weapon.weaponCount.ToString("0");
        SelectUI.SetActive(false);
    }
}
