using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct WeaponTable
{
    public SO_Weapon[] weapons;
}

public class EquipmentManager : MonoBehaviour
{
    private static EquipmentManager _instance;
    public static EquipmentManager Instance
    {
        get
        {
            _instance = FindAnyObjectByType<EquipmentManager>();
            if (Instance == null)
            {
                Debug.LogError("씬에 스크립트를 참조한 오브젝트가 없습니다");
            }
            return _instance;
        }
    }
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
    public Image[] WeaponBackgroundImage;
    public Image[] WeaponSlotImage;
    public Text[] WeaponCountText;
    public Text SynthesisCountText;

    private Color[] SynthesisColor = { Color.red, Color.cyan };
    private int synthesisCount = 1;

    [Header("강화")]
    [Header("인벤토리")]
    public WeaponTable[] WeaponArray; // 2중 배열로 무기 종류와 등급에 따른 무기 분류
    public Slot[] Invens;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    public void OpenItemInfoPage(Slot inven)
    {
        weaponTypeValue = (int)inven._WeaponType;
        weaponLevelValue = (int)inven._WeaponLevel;
        ItemInfoPanel.SetActive(true);
        RefreshSynthesis();
    }

    public void RefreshSlot()
    {
        for (int i = 0; i < Invens.Length; i++)
        {
            Invens[i].RefreshIcon();
        }
    }

    private void RefreshSynthesis()
    {
        if (WeaponArray[weaponTypeValue].weapons.Length < weaponLevelValue + 1)
            return;
        for (int i = 0; i < 2; i++)
        {
            WeaponBackgroundImage[i].color = weaponLevelColor[weaponLevelValue + i];
            WeaponSlotImage[i].sprite = WeaponArray[weaponTypeValue]
                .weapons[weaponLevelValue + i]
                .WeaponIcon;
            string hexColor = "#" + ColorUtility.ToHtmlStringRGB(SynthesisColor[i]);
            int weaponValue = synthesisCount * (i == 0 ? 5 : 1);
            WeaponCountText[i].text =
                $"{WeaponArray[weaponTypeValue].weapons[weaponLevelValue + i].weaponCount}(<color={hexColor}>{(weaponValue >= 0 ? "+" : "-")}{weaponValue}</color>)";
            SynthesisCountText.text = synthesisCount.ToString("0");
        }
    }

    public SO_Weapon GetWeapon(WeaponType type, WeaponLevel level) =>
        WeaponArray[(int)type].weapons[(int)level];

    public void UpCount()
    {
        if (
            (synthesisCount + 1) * 5
            <= WeaponArray[weaponTypeValue].weapons[weaponLevelValue].weaponCount
        )
        {
            synthesisCount++;
            RefreshSynthesis();
        }
    }

    public void DownCount()
    {
        if (synthesisCount - 1 > 0)
        {
            synthesisCount--;
            RefreshSynthesis();
        }
    }

    public void Synthesis()
    {
        if (WeaponArray[weaponTypeValue].weapons.Length < weaponLevelValue + 1)
            return;
        if (
            synthesisCount * 5
            <= WeaponArray[weaponTypeValue].weapons[weaponLevelValue].weaponCount
        )
        {
            WeaponArray[weaponTypeValue].weapons[weaponLevelValue].weaponCount -=
                (uint)synthesisCount * 5;
            GetItem(weaponTypeValue, weaponLevelValue + 1, (uint)synthesisCount);
            synthesisCount = 1;
            RefreshSynthesis();
        }
    }

    public void GetItem(int weaponType, int weaponLevel, uint amount)
    {
        WeaponArray[weaponType].weapons[weaponLevel].weaponCount += amount;
        RefreshSlot();
    }
}
