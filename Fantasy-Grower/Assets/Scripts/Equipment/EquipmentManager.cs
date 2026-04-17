using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Career
{
    Warrior,
    Archer,
    Wizard,
}

[System.Serializable]
public struct WeaponIcon
{
    public Career career;
    public Sprite[] icons;
}

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

    [Tooltip("낮은 index일 수록 높은 등급의 색으로 해주세요")]
    public Color[] weaponLevelColor = new Color[5];

    [Header("임시 변수(실 사용 시 삭제 바람)")]
    public Career career;

    [Header("장비 탭")]
    [SerializeField]
    private WeaponIcon[] WeaponIcons = new WeaponIcon[3];

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
    public SO_Weapon[] weapons = new SO_Weapon[(int)WeaponID.D2 + 1];
    public Slot[] Invens;

    [Header("대장간 변수")]
    public GameObject SmithyTab;

    [Header("합성")]
    public int SynthCount;
    public SynthSlot[] SynthSlots;

    private WeaponID synthID;

    private SO_Weapon currentWeapon;

    private Sprite[] WeaponSprite = new Sprite[2];

    private Dictionary<WeaponID, int> weaponBGColorMap = new Dictionary<WeaponID, int>
    {
        { WeaponID.S1, 0 },
        { WeaponID.S2, 0 },
        { WeaponID.A1, 1 },
        { WeaponID.A2, 1 },
        { WeaponID.B1, 2 },
        { WeaponID.B2, 2 },
        { WeaponID.C1, 3 },
        { WeaponID.C2, 3 },
        { WeaponID.D1, 4 },
        { WeaponID.D2, 4 },
    };

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
        for (int i = 0; i < WeaponSprite.Length; i++)
        {
            WeaponSprite[i] = WeaponIcons[(int)career].icons[i];
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
        if (true && currentWeapon != null)
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
        GetInfoText.text = currentWeapon.weaponInfo;
    }

    public void RefreshSlot()
    {
        for (int i = 0; i < Invens.Length; i++)
        {
            Invens[i].RefreshIcon();
        }
    }

    public void SaveSynthWeapon(WeaponID ID)
    {
        if (ID < WeaponID.A2)
            return;
        for (int i = 0; i < SynthSlots.Length; i++)
        {
            if (SynthSlots[i].isSelectSlot)
            {
                synthID = ID;
                SO_Weapon temp = GetWeapon(ID);
                currentWeapon = temp;
                SynthSlots[i].SwapImage(temp, GetIcon(ID), GetColor(ID));
            }
            else
            {
                WeaponID nextWeaponID = (WeaponID)(ID - 2);
                SynthSlots[i]
                    .SwapImage(GetWeapon(nextWeaponID), GetIcon(ID), GetColor(nextWeaponID));
            }
        }
    }

    public SO_Weapon GetWeapon(WeaponID weaponID) => weapons[(int)weaponID];

    public Sprite GetIcon(WeaponID ID)
    {
        int iconCode = (int)ID % 2;
        return WeaponSprite[iconCode];
    }

    public Color GetColor(WeaponID ID)
    {
        weaponBGColorMap.TryGetValue(ID, out int index);
        return weaponLevelColor[index];
    }

    public bool CanSynth(WeaponID ID) => weapons[(int)ID].weaponCount >= SynthCount;

    public void Synthesis()
    {
        if (currentWeapon != null && SynthCount > 0 && synthID >= WeaponID.A1 && CanSynth(synthID))
        {
            uint synthAmount = (uint)(currentWeapon.weaponCount / SynthCount);
            currentWeapon.weaponCount = (uint)(currentWeapon.weaponCount % SynthCount);
            GetWeapon((WeaponID)(synthID - 2)).weaponCount += synthAmount;
            SaveSynthWeapon(synthID);
        }
    }

    public void GetItem(WeaponID id, uint amount)
    {
        weapons[(int)id].weaponCount += amount;
        RefreshSlot();
    }

    public void ResetWeapon() => currentWeapon = null;
}
