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
    private int synthesisCount = 1;

    [Header("강화")]
    [Header("인벤토리")]
    public WeaponTable[] weaponArray; // 2중 배열로 무기 종류와 등급에 따른 무기 분류
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
        RefreshSynthesis();
    }

    public void RefreshSlot()
    {
        for (int i = 0; i < invens.Length; i++)
        {
            invens[i].RefreshIcon();
        }
    }

    private void RefreshSynthesis()
    {
        for (int i = 0; i < 2; i++)
        {
            weaponBackgroundImage[i].color = weaponLevelColor[weaponLevelValue + i];
            weaponSlotImage[i].sprite = weaponArray[weaponTypeValue]
                .weapons[weaponLevelValue + i]
                .weaponIcon;
            string hexColor = "#" + ColorUtility.ToHtmlStringRGB(synthesisColor[i]);
            weaponCountText[i].text =
                $"{weaponArray[weaponTypeValue].weapons[weaponLevelValue + i].weaponCount}(<color={hexColor}>{synthesisCount * 5 * (i == 0 ? -1 : (1 / 5f))}</color>)";
            synthesisCountText.text = synthesisCount.ToString("0");
        }
    }

    public SO_Weapon GetWeapon(WeaponType type, WeaponLevel level) =>
        weaponArray[(int)type].weapons[(int)level];

    public void UpCount()
    {
        if (
            (synthesisCount + 1) * 5
            <= weaponArray[weaponTypeValue].weapons[weaponLevelValue].weaponCount
        )
        {
            Debug.Log("dslfksjdf");
            synthesisCount++;
            RefreshSynthesis();
        }
    }

    public void DownCount()
    {
        if (synthesisCount - 1 > 0)
        {
            Debug.Log("dslfksjdf");
            synthesisCount--;
            RefreshSynthesis();
        }
    }

    public void Synthesis()
    {
        if (synthesisCount * 5 < weaponArray[weaponTypeValue].weapons[weaponLevelValue].weaponCount)
        {
            weaponArray[weaponTypeValue].weapons[weaponLevelValue].weaponCount -=
                (uint)synthesisCount * 5;
            GetItem(weaponTypeValue, weaponLevelValue + 1, (uint)synthesisCount);
            synthesisCount = 1;
            RefreshSynthesis();
        }
    }

    public void GetItem(int weaponType, int weaponLevel, uint amount)
    {
        weaponArray[weaponType].weapons[weaponLevel].weaponCount += amount;
        RefreshSlot();
    }
}
