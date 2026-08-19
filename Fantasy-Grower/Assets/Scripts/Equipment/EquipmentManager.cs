using System.Collections.Generic;
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

    [SerializeField]
    private int[] MithrilAmount = { 50, 75, 120, 180, 300 };

    [SerializeField]
    private float[] GradeScrollV = { 3.0f, 2.2f, 1.7f, 1.3f, 1.0f };

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

    private GameManager boundGameManager;
    private bool isInitialized;

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

        BindGameManager();
        if (boundGameManager != null)
            career = boundGameManager.SelectedJob;

        RebuildWeaponCache();
        isInitialized = true;
        SceneChanger.SceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        BindGameManager();
        RefreshStatModifiers();
    }

    private void OnDestroy()
    {
        if (_instance != this)
            return;

        SceneChanger.SceneLoaded -= HandleSceneLoaded;
        UnbindGameManager();
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
        weaponMap.Clear();
        weaponBGColorMap.Clear();

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
        RefreshEquipmentUI();
    } //버튼 추가 형

    public void Unequip()
    {
        if (EquipWeapon == null)
            return;

        EquipWeapon = null;
        RefreshStatModifiers();
        RefreshEquipmentUI();
    } //버튼 추가 형

    public void UpgradeWeapon()
    {
        //재화 관리 매니저에서 강화스크롤 비교 후 사용 메서드 활용하여 재화 사용
        if (currentWeapon != null && maxUpgradeLevel > currentWeapon.weaponLevel)
        {
            if (
                GoodsManager
                    .Instance.GetGoods(GoodsType.UpgradeScroll)
                    .Decrease((uint)useScrollAmount)
            )
            {
                currentWeapon.weaponLevel++;
                RefreshInfo();
                RefreshInfoInUpgradeTab();
                RefreshStatModifiers();
                EquipmentUIManager.Instance.RefreshSlotUI();
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
        EquipmentUIManager.Instance.SettingEquipStateUI(IsEquipped(inven.ID));
    }

    private int GetRequiredScrolls(int targetLevel)
    {
        int L = targetLevel + 1;

        if (L < 1 || L > 20)
            return 0; // 범위 외 예외 처리

        if (L >= 1 && L <= 4)
        {
            return 5 * L;
        }
        else if (L >= 5 && L <= 12)
        {
            // 2.5 * L * L - 12.5 * L + 30 을 정수 연산으로 변환
            return (5 * L * L - 25 * L + 60) / 2;
        }
        else // 13레벨 ~ 20레벨 구간
        {
            // 3차 식 통분 처리 (마지막에 3으로 나눔)
            return (5 * L * L * L - 165 * L * L + 1915 * L - 6570) / 3;
        }
    }

    public int useScrollAmount =>
        Mathf.RoundToInt(
            GetRequiredScrolls(currentWeapon.weaponLevel)
                * GradeScrollV[((int)currentWeapon.WeaponID / 2)]
        );

    private uint AwakeUseMithril() => (uint)MithrilAmount[currentWeapon.weaponAwakeLevel];

    private uint GetCurrentAwakeMithrilAmount()
    {
        if (
            currentWeapon == null
            || currentWeapon.weaponAwakeLevel >= maxAwakeLevel
            || currentWeapon.weaponAwakeLevel >= MithrilAmount.Length
        )
        {
            return 0;
        }

        return AwakeUseMithril();
    }

    private bool TrySpendAwakeMithril()
    {
        uint mithrilAmount = GetCurrentAwakeMithrilAmount();
        return mithrilAmount > 0
            && GoodsManager.Instance.GetGoods(GoodsType.Mithril).Decrease(mithrilAmount);
    }

    void RefreshInfo()
    {
        if (currentWeapon.weaponLevel > 0)
        {
            EquipmentUIManager.Instance.WeaponLevelText.SetText("+{0}", currentWeapon.weaponLevel);
        }
        else
        {
            EquipmentUIManager.Instance.WeaponLevelText.SetText("");
        }

        EquipmentUIManager.Instance.EquipInfoText.SetText(
            "공격력: {0}%",
            currentWeapon.equipDamage * 100f
        );
        EquipmentUIManager.Instance.GetInfoText.SetText(
            "공격력: {0}%",
            currentWeapon.getDamage * 100f
        );
    }

    void RefreshInfoInUpgradeTab()
    {
        if (currentWeapon.weaponLevel > 0)
        {
            EquipmentUIManager.Instance.UpgradeWeaponLevelText.SetText(
                "+{0}",
                currentWeapon.weaponLevel
            );
            if (currentWeapon.weaponLevel >= maxUpgradeLevel)
            {
                EquipmentUIManager.Instance.UpgradeWeaponLevelUpText.SetText("");
            }
        }
        else
        {
            EquipmentUIManager.Instance.WeaponLevelText.SetText("");
        }

        EquipmentUIManager.Instance.SettingUpgradeCostUI(currentWeapon);

        EquipmentUIManager.Instance.EquipInfoText.SetText(
            "공격력: {0}%",
            currentWeapon.equipDamage * 100f
        );
        EquipmentUIManager.Instance.GetInfoText.SetText(
            "공격력: {0}%",
            currentWeapon.getDamage * 100f
        );
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

        EquipmentUIManager.Instance.SettingAwakeCostUI(
            currentWeapon,
            GetCurrentAwakeMithrilAmount()
        );
    }

    public SO_Weapon GetWeapon(WeaponID weaponID) => weaponMap[weaponID];

    public bool IsEquipped(WeaponID weaponID)
    {
        if (EquipWeapon == null)
            return false;

        return EquipWeapon == GetWeapon(weaponID);
    }

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
            EquipmentUIManager.Instance.RefreshSlotUI();
            RefreshStatModifiers();
        }
    } //버튼 추가 형

    public void Awakening()
    {
        while (
            CheckWeaponState
            && currentWeapon.weaponAwakeLevel < maxAwakeLevel
            && TrySpendAwakeMithril()
        )
        {
            currentWeapon.Decrease((uint)useWeaponCount);
            currentWeapon.weaponAwakeLevel++;
        }
        SaveSelectAwakeWeapon(currentSelectID);
        EquipmentUIManager.Instance.RefreshSlotUI();
        RefreshStatModifiers();
    } //버튼 추가 형

    /// <summary>
    /// 현재 플레이어에게 장착 무기 효과와 해금 무기 보유 효과를 각각 갱신합니다.
    /// 플레이어가 교체되면 이전 플레이어의 핸들은 재사용할 수 없으므로 새로 발급받습니다.
    /// </summary>
    private void RefreshStatModifiers()
    {
        BindGameManager();

        Entity player =
            boundGameManager != null ? boundGameManager.GetPlayer()
            : GameManager.InstanceOrNull != null ? GameManager.InstanceOrNull.GetPlayer()
            : null;
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
        BindGameManager();
        RefreshStatModifiers();
    }

    private void BindGameManager()
    {
        GameManager gameManager = GameManager.InstanceOrNull;
        if (gameManager == boundGameManager)
            return;

        UnbindGameManager();
        boundGameManager = gameManager;

        if (boundGameManager != null)
        {
            boundGameManager.OnPlayerChanged += HandlePlayerChanged;
            boundGameManager.OnSelectedJobChanged += HandleSelectedJobChanged;

            if (isInitialized)
                ChangeCareer(boundGameManager.SelectedJob);
        }
    }

    private void UnbindGameManager()
    {
        if (boundGameManager == null)
            return;

        boundGameManager.OnPlayerChanged -= HandlePlayerChanged;
        boundGameManager.OnSelectedJobChanged -= HandleSelectedJobChanged;
        boundGameManager = null;
    }

    private void HandlePlayerChanged(Entity player)
    {
        RefreshStatModifiers();
    }

    private void HandleSelectedJobChanged(Career job)
    {
        ChangeCareer(job);
    }

    private void ChangeCareer(Career newCareer)
    {
        if (career == newCareer && weaponMap.Count > 0)
            return;

        career = newCareer;
        currentWeapon = null;
        currentSelectID = default;
        EquipWeapon = null;

        RemoveStatModifiers();
        RebuildWeaponCache();

        if (EquipmentUIManager.Instance != null)
        {
            EquipmentUIManager.Instance.RefreshSlotUI();
            EquipmentUIManager.Instance.SettingEquipStateUI(false);
            EquipmentUIManager.Instance.ClearAwakeCostUI();
        }

        RefreshStatModifiers();
    }

    private void RefreshEquipmentUI()
    {
        if (EquipmentUIManager.Instance == null)
            return;

        EquipmentUIManager.Instance.RefreshSlotUI();
        EquipmentUIManager.Instance.SettingEquipStateUI(
            currentWeapon != null && EquipWeapon == currentWeapon
        );
    }

    private void RebuildWeaponCache()
    {
        ResetWeaponDicionary();
        AssignIcon();
    }

    public void GetItem(WeaponID id, uint amount)
    {
        weaponMap[id].Increase(amount);
        if (EquipmentUIManager.Instance != null && currentWeapon != null)
            RefreshInfo();

        RefreshStatModifiers();
    }

    public void ResetWeapon()
    {
        currentWeapon = null;

        if (EquipmentUIManager.Instance != null)
        {
            EquipmentUIManager.Instance.SettingEquipStateUI(false);
            EquipmentUIManager.Instance.ClearAwakeCostUI();
        }
    }
}
