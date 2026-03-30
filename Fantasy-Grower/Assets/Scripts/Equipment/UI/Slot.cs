using UnityEngine;
using UnityEngine.UI;

public enum SlotType
{
    Inventory,
    Synthesis,
    Enchent,
}

public abstract class Slot : MonoBehaviour
{
    [Header("酒捞袍 加己")]
    public SO_Weapon weapon; //公扁 SO

    [Header("UI加己")]
    public Image weaponIcon;
    public SlotType slotType;

    protected Color[] weaponLevelColor = { Color.green, Color.cyan, Color.magenta, Color.yellow };

    protected void Awake()
    {
        RefreshIcon();
        if (TryGetComponent<Button>(out Button button))
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    protected abstract void OnButtonClick();

    public abstract void RefreshIcon();
}
