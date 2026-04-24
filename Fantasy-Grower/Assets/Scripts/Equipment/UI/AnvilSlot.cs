using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnvilSlot : MonoBehaviour, IPointerClickHandler
{
    [Header("SlotUI속성")]
    [SerializeField]
    private Image SlotImage;

    [SerializeField]
    private Image BGImage;

    [SerializeField]
    private Text WeaponCountText; //TMP로 바뀔 예정

    [SerializeField]
    private GameObject[] AwakeImage;

    [SerializeField]
    private bool isAwake;

    [Header("선택 UI 속성")]
    [SerializeField]
    private GameObject SelectUI;

    [SerializeField]
    private GameObject[] SelectSlots;

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

    public void ShowAwakeImage(int awakeLevel)
    {
        for (int i = 0; i < awakeLevel; i++)
        {
            AwakeImage[i].SetActive(true);
        }
    }

    public void SwapImage(SO_Weapon weapon, Sprite icon, Color _color)
    {
        SlotImage.sprite = icon;
        BGImage.color = _color;
        WeaponCountText.text = weapon.weaponCount.ToString("0");
        SelectUI.SetActive(false);
    }
}
