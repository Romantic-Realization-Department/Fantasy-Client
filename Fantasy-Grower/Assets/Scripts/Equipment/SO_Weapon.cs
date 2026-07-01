using UnityEngine;

public enum WeaponID
{
    S1,
    S2,
    A1,
    A2,
    B1,
    B2,
    C1,
    C2,
    D1,
    D2,
}

[CreateAssetMenu(fileName = "SO_Equipment", menuName = "ScriptableObjects/SO_Equipment", order = 6)]
public class SO_Weapon : SO_Goods
{
    protected override string GoodsName => "무기";

    public override void Increase(uint amount)
    {
        isUnlock = true;
        base.Increase(amount);
    }

    //필요한 기능에 따라 변수 및 함수가 추가될 수 있음
    [Header("필요 변수")]
    [SerializeField]
    private WeaponID _weaponID;
    public WeaponID WeaponID => _weaponID;

    [SerializeField]
    private WeaponID _nextWeaponID;
    public WeaponID NextWeaponID => _nextWeaponID;

    [Header("무기 정보")]
    public string weaponName;

    [field: SerializeField]
    public int equipDamage { get; private set; }

    [field: SerializeField]
    public int getDamage { get; private set; }

    public int DefaultDamage() => Mathf.RoundToInt(getDamage * levelAmount * AwakeAmount);

    public int EquipDamage() => Mathf.RoundToInt(equipDamage * levelAmount * AwakeAmount);

    public int weaponLevel;
    private float levelAmount
    {
        get => 1 + (weaponLevel * .04f);
    }
    public int weaponAwakeLevel;
    private float AwakeAmount
    {
        get => Mathf.Pow(1.3f, weaponAwakeLevel);
    }
    public bool isUnlock;
}
