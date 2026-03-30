using UnityEngine;
using UnityEngine.UI;

public class Slot_Synthesis : Slot
{
    protected override void OnButtonClick()
    {
        if (weapon == null)
            return;
        Debug.LogError("아직 구현 안됨");
        // EquipmentManager와 연결하여서 다시 아이템을 원래 인벤토리 칸으로 들어가도록 설정
    }

    public override void RefreshIcon()
    {
        Image currentImage = GetComponent<Image>();
        if (weapon == null)
        {
            weaponIcon.sprite = null;
            weaponIcon.color = new Color(0, 0, 0, 0);
            currentImage.color = Color.white;
        }
        else
        {
            weaponIcon.sprite = weapon.weaponIcon;
            weaponIcon.color = Color.white;
            currentImage.color = weaponLevelColor[(int)weapon.currentLevel];
        }
    }
}
