using System.Collections.Generic;
using UnityEngine;

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
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<EquipmentManager>();
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

    [Space(20f)]
    [SerializeField]
    private int _maxUpgradeLevel;
    public int maxUpgradeLevel => _maxUpgradeLevel;
    private SO_Weapon EquipWeapon;

    [Header("인벤토리")]
    public Weapon[] weapons = new Weapon[(int)Career.Wizard + 1];

    [Header("대장간 변수")]
    public int useWeaponCount;
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

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void TestButton()
    {
        for (int i = 0; i < weaponMap.Count; i++)
        {
            weaponMap[(WeaponID)i].Increase(100);
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
    } //버튼 추가 형

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
    } //버튼 추가 형

    public void OpenItemInfoPage(Slot inven)
    {
        currentWeapon = GetWeapon(inven.ID);
        EquipmentUIManager.Instance.WeaponIconImage.sprite = GetIcon(inven.ID);
        //WeaponBGImage.color =

        EquipmentUIManager.Instance.WeaponInfoObject.SetActive(true);
    }

    void RefreshInfo()
    {
        if (currentWeapon.weaponLevel > 0)
        {
            EquipmentUIManager.Instance.WeaponLevelText.text =
                "+" + currentWeapon.weaponLevel.ToString("0");
        }
        else
        {
            EquipmentUIManager.Instance.WeaponLevelText.text = "";
        }

        EquipmentUIManager.Instance.EquipInfoText.text =
            "공격력: " + (currentWeapon.equipDamage * 100f).ToString("0") + "%";
        EquipmentUIManager.Instance.GetInfoText.text = currentWeapon.weaponInfo;
    }

    public void SaveSelectWeapon(WeaponID ID, SelectSlotType _SelectSlotType)
    {
        switch (_SelectSlotType)
        {
            case SelectSlotType.Synth:
                SaveSelectSynthWeapon(ID);
                break;
            case SelectSlotType.Awake:
                SaveSelectAwakeWeapon(ID);
                break;
        }
    }

    public void SaveSelectSynthWeapon(WeaponID ID)
    {
        if (ID < WeaponID.A2)
            return;
        for (int i = 0; i < EquipmentUIManager.Instance.SynthSlots.Length; i++)
        {
            if (EquipmentUIManager.Instance.SynthSlots[i].IsSelectSlot)
            {
                currentSelectID = ID;
                SO_Weapon temp = GetWeapon(ID);
                currentWeapon = temp;
                EquipmentUIManager
                    .Instance.SynthSlots[i]
                    .SwapImage(temp, GetIcon(ID), GetColor(ID));
            }
            else
            {
                WeaponID nextWeaponID = (WeaponID)(ID - 2);
                EquipmentUIManager
                    .Instance.SynthSlots[i]
                    .SwapImage(GetWeapon(nextWeaponID), GetIcon(ID), GetColor(nextWeaponID));
            }
        }
    }

    public void SaveSelectAwakeWeapon(WeaponID ID)
    {
        currentSelectID = ID;
        currentWeapon = GetWeapon(ID);
        for (int i = 0; i < EquipmentUIManager.Instance.AwakeSlots.Length; i++)
        {
            EquipmentUIManager.Instance.AwakeSlots[i].HideAwakeImage();
            EquipmentUIManager
                .Instance.AwakeSlots[i]
                .SwapImage(currentWeapon, GetIcon(ID), GetColor(ID));
            EquipmentUIManager
                .Instance.AwakeSlots[i]
                .ShowAwakeImage(currentWeapon.weaponAwakeLevel + i);
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

    public bool CanUse(WeaponID ID) => weaponMap[ID].Get() >= useWeaponCount;

    bool CheckWeaponState => currentWeapon != null && useWeaponCount > 0 && CanUse(currentSelectID);

    public void Synthesis()
    {
        if (CheckWeaponState && currentSelectID >= WeaponID.A1)
        {
            uint synthAmount = (uint)(currentWeapon.Get() / useWeaponCount);
            currentWeapon.Decrease(
                (uint)(currentWeapon.Get() - (currentWeapon.Get() % useWeaponCount))
            );
            GetWeapon(currentWeapon.NextWeaponID).Increase(synthAmount);
            SaveSelectSynthWeapon(currentSelectID);
        }
    } //버튼 추가 형

    public void Awakening()
    {
        if (CheckWeaponState && currentWeapon.weaponAwakeLevel < maxAwakeLevel)
        {
            int possibleAwakeCount = (int)(currentWeapon.weaponAwakeLevel / (int)useWeaponCount);
            int remainingAwakeLevel = maxAwakeLevel - currentWeapon.weaponAwakeLevel;
            int actualAwakeCount = Mathf.Min(possibleAwakeCount, remainingAwakeLevel);

            currentWeapon.Decrease((uint)(actualAwakeCount * useWeaponCount));
            currentWeapon.weaponAwakeLevel += actualAwakeCount;
        }
        SaveSelectAwakeWeapon(currentSelectID);
    } //버튼 추가 형

    public void ResetWeapon() => currentWeapon = null;
}
