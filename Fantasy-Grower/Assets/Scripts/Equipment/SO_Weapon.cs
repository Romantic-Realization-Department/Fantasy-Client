using UnityEngine;

public enum WeaponLevel
{
    C,
    D,
    B,
    A,
    S,
}

public enum WeaponType
{
    Rapier,
    LongSword,
    GreatSword,
    GreatBow,
    CrossBow,
    Staff,
    Grimoire,
}

[CreateAssetMenu(fileName = "SO_Equipment", menuName = "ScriptableObjects/SO_Equipment", order = 6)]
public class SO_Weapon : ScriptableObject
{
    //필요한 기능에 따라 변수 및 함수가 추가될 수 있음
    [Header("필요 변수")]
    [SerializeField]
    private Sprite _weaponIcon;
    public Sprite WeaponIcon => _weaponIcon;

    [SerializeField]
    private string _weaponID;
    public string WeaponID => _weaponID;

    [Header("무기 정보")]
    public string weaponName;
    public string weaponInfo;
    public uint weaponCount
    {
        get { return weaponCount; }
        set
        {
            isUnlock = true;
            weaponCount = value;
        }
    }
    public int Damage;
    public bool isUnlock;
}
