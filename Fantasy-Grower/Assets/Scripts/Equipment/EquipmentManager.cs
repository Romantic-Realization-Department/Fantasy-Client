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

[System.Serializable]
public struct Weapon
{
    public Career career;
    public SO_Weapon[] weapon;
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
    [SerializeField]
    private Color[] weaponLevelColor = new Color[5];

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
    public Weapon[] weapons = new Weapon[(int)Career.Wizard + 1];
    public Slot[] Invens;

    [Header("대장간 변수")]
    public GameObject SmithyTab;
    public int useWeaponCount;

    [Header("합성")]
    public AnvilSlot[] SynthSlots;

    [Header("각성")]
    public AnvilSlot[] AwakeSlots;
    public int maxAwakeLevel;

    private WeaponID currentSelectID;

    private SO_Weapon currentWeapon;

    private Sprite[] WeaponSprite = new Sprite[2];

    private Dictionary<WeaponID, SO_Weapon> weaponMap = new Dictionary<WeaponID, SO_Weapon>();

    private Dictionary<WeaponID, Color> weaponBGColorMap = new Dictionary<WeaponID, Color>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
        ResetWeaponDicionary();
        AssignIcon();
    }

    public void TestButton()
    {
        for (int i = 0; i < weaponMap.Count; i++)
        {
            weaponMap[(WeaponID)i].weaponCount = 100;
        }
    }

    private void ResetWeaponDicionary()
    {
        for (int i = 0; i <= (int)WeaponID.D2; i++)
        {
            weaponBGColorMap.Add((WeaponID)i, weaponLevelColor[i / 2]);
            weaponMap.Add((WeaponID)i, weapons[(int)career].weapon[i]);
        }
    } //무기의 종류가 2개일 경우만 해당, 3개 이상이 되면 수정 필요

    private void AssignIcon()
    {
        for (int i = 0; i < WeaponSprite.Length; i++)
        {
            WeaponSprite[i] = WeaponIcons[(int)career].icons[i];
        }
    }

    public void Equip()
    {
        EquipWeapon = currentWeapon;
    }

    public void UpgradeWeapon()
    {
        //재화 관리 매니저에서 강화스크롤 비교 후 사용 메서드 활용하여 재화 사용
        if (true && currentWeapon != null)
        {
            if (maxUpgradeLevel > currentWeapon.weaponLevel)
            {
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

    public void SaveSelectWeapon(WeaponID ID, bool isSynth)
    {
        if (isSynth)
            SaveSelectSynthWeapon(ID);
        else
            SaveSelectAwakeWeapon(ID);
    }

    public void SaveSelectSynthWeapon(WeaponID ID)
    {
        if (ID < WeaponID.A2)
            return;
        for (int i = 0; i < SynthSlots.Length; i++)
        {
            if (SynthSlots[i].isSelectSlot)
            {
                currentSelectID = ID;
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

    public void SaveSelectAwakeWeapon(WeaponID ID)
    {
        currentSelectID = ID;
        currentWeapon = GetWeapon(ID);
        for (int i = 0; i < AwakeSlots.Length; i++)
        {
            AwakeSlots[i].HideAwakeImage();
            AwakeSlots[i].SwapImage(currentWeapon, GetIcon(ID), GetColor(ID));
            AwakeSlots[i].ShowAwakeImage(currentWeapon.weaponAwakeLevel + i);
        }
    }

    public SO_Weapon GetWeapon(WeaponID weaponID) => weaponMap[weaponID];

    public Sprite GetIcon(WeaponID ID)
    {
        int iconCode = (int)ID % 2;
        return WeaponSprite[iconCode];
    }

    public Color GetColor(WeaponID ID)
    {
        weaponBGColorMap.TryGetValue(ID, out Color _color);
        return _color;
    }

    public bool CanUse(WeaponID ID) => weaponMap[ID].weaponCount >= useWeaponCount;

    bool CheckWeaponState => currentWeapon != null && useWeaponCount > 0 && CanUse(currentSelectID);

    public void Synthesis()
    {
        if (CheckWeaponState && currentSelectID >= WeaponID.A1)
        {
            uint synthAmount = (uint)(currentWeapon.weaponCount / useWeaponCount);
            currentWeapon.weaponCount = (uint)(currentWeapon.weaponCount % useWeaponCount);
            GetWeapon((WeaponID)(currentSelectID - 2)).weaponCount += synthAmount;
            SaveSelectSynthWeapon(currentSelectID);
        }
    }

    public void Awakening()
    {
        for (int i = 0; CheckWeaponState && currentWeapon.weaponAwakeLevel < maxAwakeLevel; i++)
        {
            currentWeapon.weaponCount = (uint)(currentWeapon.weaponCount - useWeaponCount);
            currentWeapon.weaponAwakeLevel++;
        }
        SaveSelectAwakeWeapon(currentSelectID);
    }

    public void GetItem(WeaponID id, uint amount)
    {
        weaponMap[id].weaponCount += amount;
        RefreshSlot();
    }

    public void ResetWeapon() => currentWeapon = null;
}
