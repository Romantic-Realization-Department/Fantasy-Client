using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentManager : MonoBehaviour
{
    private static EquipmentManager _instance;
    public static EquipmentManager Instance
    {
        get
        {
            _instance = FindAnyObjectByType<EquipmentManager>();
            if (_instance == null)
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

    [Header("임시 변수(실 사용 시 삭제 바람)")]
    [Range(0, 2)]
    public int career;

    [Header("장비 탭")]
    [SerializeField]
    private Sprite[] WeaponIcon = new Sprite[6];

    [SerializeField]
    private GameObject WeaponInfoObject;

    [Header("강화")]
    [SerializeField]
    private Image WeaponIconImage;

    [SerializeField]
    private Image WeaponBGImage;

    [SerializeField]
    private Text WeaponLevelText;

    [SerializeField]
    private Text EquipInfoText;

    [SerializeField]
    private Text GetInfoText;

    [SerializeField]
    private GameObject[] AwakeObject;

    [Space(20f)]
    [SerializeField]
    private int _maxUpgradeLevel;
    public int maxUpgradeLevel => _maxUpgradeLevel;
    private SO_Weapon EquipWeapon;

    [Header("인벤토리")]
    public SO_Weapon[] weapons = new SO_Weapon[(int)WeaponID.Count];
    public Slot[] Invens;

    [Header("대장간 변수")]
    public GameObject SmithyTab;

    [Header("합성")]
    public Image[] WeaponBackgroundImage;
    public Image[] WeaponSlotImage;
    public Text[] WeaponCountText;
    public Text SynthesisCountText;

    private Color[] SynthesisColor = { Color.red, Color.cyan };
    private Dictionary<int, Sprite> WeaponIconDic = new Dictionary<int, Sprite>();
    private int synthesisCount = 1;
    private SO_Weapon currentWeapon;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
        AssignIcon();
    }

    private void AssignIcon()
    {
        for (int i = 0; i < 2; i++)
        {
            WeaponIconDic.Add(i, WeaponIcon[i + career * 2]);
        }
    }

    public void Equip()
    {
        Debug.Log("장착");
        EquipWeapon = currentWeapon;
    }

    public void UpgradeWeapon()
    {
        //재화 관리 매니저에서 강화스크롤 비교 후 사용 메서드 활용하여 재화 사용
        if (true)
        {
            if (maxUpgradeLevel > currentWeapon.weaponLevel)
            {
                Debug.Log("강화");
                currentWeapon.weaponLevel++;
                RefreshInfo();
            }
        }
    }

    public void OpenItemInfoPage(Slot inven)
    {
        currentWeapon = GetWeapon(inven.ID);
        WeaponIconImage.sprite = GetIcon(inven.ID);
        //WeaponBGImage.color =

        WeaponInfoObject.SetActive(true);

        //ItemInfoPanel.SetActive(true);
        //RefreshSynthesis();
    }

    void RefreshInfo()
    {
        if (currentWeapon.weaponLevel > 0)
        {
            WeaponLevelText.text = "+" + currentWeapon.weaponLevel.ToString("0");
        }
        else
        {
            WeaponLevelText.text = "";
        }

        EquipInfoText.text = "공격력: " + (currentWeapon.equipDamage * 100f).ToString("0") + "%";
        GetInfoText.text = 
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
        //수정 필요
        //for (int i = 0; i < 2; i++)
        //{
        //    WeaponBackgroundImage[i].color = weaponLevelColor[(int)weapons[WeaponID].Rate + i];
        //    WeaponSlotImage[i].sprite = weapons[(int)weapons[WeaponID].Rate]
        //        .weapons[weaponLevelValue + i]
        //        .WeaponIcon;
        //    string hexColor = "#" + ColorUtility.ToHtmlStringRGB(SynthesisColor[i]);
        //    int weaponValue = synthesisCount * (i == 0 ? 5 : 1);
        //    WeaponCountText[i].text =
        //        $"{WeaponArray[(int)weapons[WeaponID].Rate].weapons[weaponLevelValue + i].weaponCount}(<color={hexColor}>{(weaponValue >= 0 ? "+" : "-")}{weaponValue}</color>)";
        //    SynthesisCountText.text = synthesisCount.ToString("0");
        //}
    }

    public SO_Weapon GetWeapon(WeaponID weaponID) => weapons[(int)weaponID];

    public Sprite GetIcon(WeaponID ID)
    {
        int iconCode = (int)ID % 2;
        return WeaponIconDic[iconCode];
    }

    public void UpCount()
    {
        //if ((synthesisCount + 1) * 5 <= weapons[WeaponID].weaponCount)
        //{
        //    synthesisCount++;
        //    RefreshSynthesis();
        //}
    }

    public void DownCount()
    {
        //if (synthesisCount - 1 > 0)
        //{
        //    synthesisCount--;
        //    RefreshSynthesis();
        //}
    }

    public void Synthesis()
    {
        //수정 필요
        //if (WeaponArray[(int)weapons[WeaponID].Rate].weapons.Length < weaponLevelValue + 1)
        //    return;
        //if (
        //    synthesisCount * 5
        //    <= WeaponArray[(int)weapons[WeaponID].Rate].weapons[weaponLevelValue].weaponCount
        //)
        //{
        //    WeaponArray[(int)weapons[WeaponID].Rate].weapons[weaponLevelValue].weaponCount -=
        //        (uint)synthesisCount * 5;
        //    GetItem((int)weapons[WeaponID].Rate, weaponLevelValue + 1, (uint)synthesisCount);
        //    synthesisCount = 1;
        //    RefreshSynthesis();
        //}
    }

    public void GetItem(WeaponID id, uint amount)
    {
        weapons[(int)id].weaponCount += amount;
        RefreshSlot();
    }
}
