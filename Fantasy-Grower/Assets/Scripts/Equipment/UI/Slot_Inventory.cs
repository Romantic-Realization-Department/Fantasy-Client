using UnityEngine;
using UnityEngine.UI;

public class Slot_Inventory : Slot
{
    [Header("아이템 속성")]
    public uint weaponCount;

    [Header("UI속성")]
    public Text weaponCountText;

    [Space(20f)]
    public bool isInven; // 위치 판단 변수(Inspector에서 사용할 예정)

    public override void RefreshIcon()
    {
        GetComponent<Image>().color = weaponLevelColor[(int)weapon.currentLevel];
        weaponIcon.sprite = weapon.weaponIcon;
        if (weaponCount > 0)
        {
            weaponIcon.color = Color.white;
        }
        else
        {
            weaponIcon.color = new Color(0, 0, 0, .3f);
        }
    }

    private void ShowInfo() { }

    protected override void OnButtonClick()
    {
        if (isInven)
        {
            ShowInfo();
        }
        else
        {
            Debug.LogError("아이템 합성/강화 전용 manager 구현 안됨");
        }
    }
}
