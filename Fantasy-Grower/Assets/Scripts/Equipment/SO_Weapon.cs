using UnityEngine;

public enum WeaponLevel
{
    C,
    D,
    B,
    A,
    S,
}

[CreateAssetMenu(
    fileName = "SO_Equipment",
    menuName = "Scriptable Objects/SO_Equipment",
    order = 6
)]
public class SO_Weapon : ScriptableObject
{
    //필요한 기능에 따라 변수 및 함수가 추가될 수 있음
    [Header("필요 변수"), SerializeField]
    public Sprite WeaponImage { get; private set; }

    [Header("무기 정보")]
    public string weaponName;
    public string weaponInfo;
    public int Damage;
    public WeaponLevel currentLevel;
}
