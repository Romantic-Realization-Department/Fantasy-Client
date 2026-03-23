using UnityEngine;
using UnityEngine.UI;

public enum SlotType
{
    Inventory,
    Synthesis,
    Enchent,
}

public class Slot : MonoBehaviour
{
    private Button button;

    public SO_Weapon weapon;
    public Image spriteImage;
    public SlotType slotType;
    public WeaponLevel level;
}
