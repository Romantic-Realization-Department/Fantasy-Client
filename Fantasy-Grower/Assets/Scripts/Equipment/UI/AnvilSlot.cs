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
    [field: SerializeField]
    public bool IsSelectSlot { get; private set; }

    private void OnEnable()
    {
        HideAwakeImage();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsSelectSlot)
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
        int limit = Mathf.Min(awakeLevel, AwakeImage.Length);
        for (int i = 0; i < limit; i++)
        {
            AwakeImage[i]?.SetActive(true);
        }
    }

    public void HideAwakeImage()
    {
        if (isAwake)
        {
            for (int i = 0; i < AwakeImage.Length; i++)
            {
                AwakeImage[i]?.SetActive(false);
            }
        }
    }

    public void SwapImage(SO_Weapon weapon, Sprite icon, Color _color)
    {
        SlotImage.sprite = icon;
        BGImage.color = _color;
        WeaponCountText.text = weapon.Get().ToString("0");
        if (IsSelectSlot)
            SelectUI.SetActive(false);
    }
}
