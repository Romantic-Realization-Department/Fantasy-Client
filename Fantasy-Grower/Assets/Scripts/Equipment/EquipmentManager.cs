using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

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
        get => _instance;
    }

    [Tooltip("낮은 index일 수록 높은 등급의 색으로 해주세요")]
    [SerializeField]
    private Color[] weaponLevelColor = new Color[5];

    /// <summary>
    /// 게임 시작 시 할당이 필요한 변수
    /// </summary>
    [SerializeField]
    private Career career;

    [Header("장비 탭")]
    [SerializeField]
    private WeaponIcon[] WeaponIcons = new WeaponIcon[3];

    [Space(20f)]
    [SerializeField]
    private int _maxUpgradeLevel;
    public int maxUpgradeLevel => _maxUpgradeLevel;
    public SO_Weapon EquipWeapon { get; private set; }

    [Header("인벤토리")]
    public Weapon[] weapons = new Weapon[(int)Career.Wizard + 1];

    [Header("대장간 변수")]
    public int useWeaponCount;
    public int maxAwakeLevel;

    // 장착 효과와 보유 효과는 서로 다른 출처이므로 각각의 핸들을 유지합니다.
    private Entity modifierTarget;
    private EntityStatModifierHandle equippedWeaponModifierHandle;
    private EntityStatModifierHandle ownedWeaponModifierHandle;

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
            transform.parent = null;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ResetWeaponDicionary();
        AssignIcon();
        SceneChanger.SceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        RefreshStatModifiers();
    }

    private void OnDestroy()
    {
        if (_instance != this)
            return;

        SceneChanger.SceneLoaded -= HandleSceneLoaded;
        RemoveStatModifiers();
        _instance = null;
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
        if (currentWeapon == null)
            return;

        EquipWeapon = currentWeapon;
        RefreshStatModifiers();
    } //버튼 추가 형

    public void UpgradeWeapon()
    {
        //재화 관리 매니저에서 강화스크롤 비교 후 사용 메서드 활용하여 재화 사용
        if (currentWeapon != null && true)
        {
            if (maxUpgradeLevel > currentWeapon.weaponLevel)
            {
                currentWeapon.weaponLevel++;
                RefreshInfo();
                RefreshInfoInUpgradeTab();
                RefreshStatModifiers();
                EquipmentUIManager.Instance.RefrashSlotUI();
            }
        }
    } //버튼 추가 형

    public void OpenItemInfoPage(Slot inven)
    {
        currentWeapon = GetWeapon(inven.ID);
        EquipmentUIManager.Instance.SettingItemInfoUI(
            GetColor(inven.ID),
            GetIcon(inven.ID),
            currentWeapon
        );
        EquipmentUIManager.Instance.SettingUpgradeItemInfoUI(
            GetColor(inven.ID),
            GetIcon(inven.ID),
            currentWeapon
        );
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
        EquipmentUIManager.Instance.GetInfoText.text =
            "공격력: " + (currentWeapon.getDamage * 100f).ToString("0") + "%";
    }

    void RefreshInfoInUpgradeTab()
    {
        if (currentWeapon.weaponLevel > 0)
        {
            EquipmentUIManager.Instance.UpgradeWeaponLevelText.text =
                "+" + currentWeapon.weaponLevel.ToString("0");
            if (currentWeapon.weaponLevel >= maxUpgradeLevel)
            {
                EquipmentUIManager.Instance.UpgradeWeaponLevelUpText.text = "";
            }
        }
        else
        {
            EquipmentUIManager.Instance.WeaponLevelText.text = "";
        }

        string EquipUpPercent = "";
        string GetUpPercent = "";

        EquipmentUIManager.Instance.EquipInfoText.text =
            "공격력: " + (currentWeapon.equipDamage * 100f).ToString("0") + EquipUpPercent + "%";
        EquipmentUIManager.Instance.GetInfoText.text =
            "공격력: " + (currentWeapon.getDamage * 100f).ToString("0") + GetUpPercent + "%";
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
            EquipmentUIManager.Instance.RefrashSlotUI();
            RefreshStatModifiers();
        }
    } //버튼 추가 형

    public void Awakening()
    {
        if (CheckWeaponState && currentWeapon.weaponAwakeLevel < maxAwakeLevel)
        {
            int possibleAwakeCount = (int)(currentWeapon.Get() / (int)useWeaponCount);
            int remainingAwakeLevel = maxAwakeLevel - currentWeapon.weaponAwakeLevel;
            int actualAwakeCount = Mathf.Min(possibleAwakeCount, remainingAwakeLevel);

            currentWeapon.Decrease((uint)(actualAwakeCount * useWeaponCount));
            currentWeapon.weaponAwakeLevel += actualAwakeCount;
            Debug.Log("slfkjsfd");
        }
        SaveSelectAwakeWeapon(currentSelectID);
        EquipmentUIManager.Instance.RefrashSlotUI();
        RefreshStatModifiers();
    } //버튼 추가 형

    /// <summary>
    /// 현재 플레이어에게 장착 무기 효과와 해금 무기 보유 효과를 각각 갱신합니다.
    /// 플레이어가 교체되면 이전 플레이어의 핸들은 재사용할 수 없으므로 새로 발급받습니다.
    /// </summary>
    private void RefreshStatModifiers()
    {
        Entity player = GameManager.Instance != null ? GameManager.Instance.GetPlayer() : null;
        if (player == null)
            return;

        BindModifierTarget(player);

        EntityStatModifier ownedWeaponModifier = new EntityStatModifier
        {
            BonusAttackPower = CalculateOwnedWeaponDamage(),
        };
        ApplyOrUpdateModifier(ref ownedWeaponModifierHandle, ownedWeaponModifier);

        if (EquipWeapon == null)
        {
            RemoveModifier(ref equippedWeaponModifierHandle);
            return;
        }

        EntityStatModifier equippedWeaponModifier = new EntityStatModifier
        {
            BonusAttackPower = EquipWeapon.EquipDamage(),
        };
        ApplyOrUpdateModifier(ref equippedWeaponModifierHandle, equippedWeaponModifier);
    }

    private int CalculateOwnedWeaponDamage()
    {
        int damage = 0;

        foreach (SO_Weapon weapon in weapons[(int)career].weapon)
        {
            if (weapon.isUnlock)
                damage += weapon.DefaultDamage();
        }

        return damage;
    }

    private void BindModifierTarget(Entity player)
    {
        if (modifierTarget == player)
            return;

        RemoveStatModifiers();
        modifierTarget = player;
    }

    private void ApplyOrUpdateModifier(
        ref EntityStatModifierHandle handle,
        EntityStatModifier modifier
    )
    {
        if (handle.IsValid && modifierTarget.UpdateStatModifier(handle, modifier))
            return;

        handle = modifierTarget.ApplyStatModifier(modifier);
    }

    private void RemoveStatModifiers()
    {
        RemoveModifier(ref equippedWeaponModifierHandle);
        RemoveModifier(ref ownedWeaponModifierHandle);
        modifierTarget = null;
    }

    private void RemoveModifier(ref EntityStatModifierHandle handle)
    {
        if (modifierTarget != null && handle.IsValid)
            modifierTarget.RemoveStatModifier(handle);

        handle = default;
    }

    private void HandleSceneLoaded()
    {
        RefreshStatModifiers();
    }

    public void GetItem(WeaponID id, uint amount)
    {
        weaponMap[id].Increase(amount);
        if (EquipmentUIManager.Instance != null && currentWeapon != null)
            RefreshInfo();

        RefreshStatModifiers();
    }

    public void ResetWeapon() => currentWeapon = null;
}
