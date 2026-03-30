using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct WeaponTable
{
    public SO_Weapon[] weapons;
}

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }
    public static Color[] weaponLevelColor =
    {
        Color.green,
        Color.cyan,
        Color.magenta,
        Color.yellow,
    };

    [Header("합성&강화 공통 변수")]
    public GameObject ItemInfoPanel;

    private int weaponTypeValue;
    private int weaponLevelValue;

    [Header("합성")]
    public Image[] weaponBackgroundImage;
    public Image[] weaponSlotImage;
    public Text[] weaponCountText;
    public Text synthesisCountText;

    private Color[] synthesisColor = { Color.red, Color.cyan };
    private uint synthesisCount;

    [Header("강화")]
    [Header("인벤토리")]
    public WeaponTable[] weaponArray;
    public Slot[] invens;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void OpenItemInfoPage(Slot inven)
    {
        weaponTypeValue = (int)inven.weaponType;
        weaponLevelValue = (int)inven.weaponLevel;
        ItemInfoPanel.SetActive(true);
    }

    private void RefreshAllUI()
    {
        for (int i = 0; i < 2; i++)
        {
            weaponBackgroundImage[i].color = weaponLevelColor[weaponLevelValue + i];
            weaponSlotImage[i].sprite = weaponArray[weaponTypeValue]
                .weapons[weaponLevelValue + i]
                .weaponIcon;
            //weaponCountText[i].text = $"{weaponArray[weaponTypeValue].weapons[weaponLevelValue].weaponCount}({synthesisColor[i]}{}{}/color)"
        }
    }

    public SO_Weapon GetWeapon(WeaponType type, WeaponLevel level) =>
        weaponArray[(int)type].weapons[(int)level];

    public void GetItem(SO_Weapon weapon)
    {
        for (int i = 0; i < invens.Length; i++)
        {
            if (
                weaponArray[(int)invens[i].weaponType].weapons[(int)invens[i].weaponLevel] == weapon
            )
            {
                invens[i].GetItem();
                return;
            }
        }
    }
}
