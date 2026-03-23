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
    private Sprite WeaponIcon;
    public Sprite weaponIcon => WeaponIcon;

    [Header("무기 정보")]
    public string weaponName;
    public string weaponInfo;
    public int Damage;
    public WeaponType Type;
    public WeaponLevel currentLevel;
}
