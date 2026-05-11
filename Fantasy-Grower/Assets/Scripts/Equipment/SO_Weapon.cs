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
public class SO_Weapon : ScriptableObject
{
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

    [TextArea]
    public string weaponInfo;

    private uint _weaponCount;
    public uint weaponCount
    {
        get { return _weaponCount; }
        set
        {
            _weaponCount = value;
            isUnlock = true;
        }
    }
    public int equipDamage;
    public int getDamage;
    public int weaponLevel;
    public int weaponAwakeLevel;
    public bool isUnlock;
}
