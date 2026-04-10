using UnityEngine;

public enum WeaponID
{
    S1,
    S2,
    A1,
    A2,
    B1,
    B2,
    c1,
    C2,
    Count,
}

[CreateAssetMenu(fileName = "SO_Equipment", menuName = "ScriptableObjects/SO_Equipment", order = 6)]
public class SO_Weapon : ScriptableObject
{
    //필요한 기능에 따라 변수 및 함수가 추가될 수 있음
    [Header("필요 변수")]
    [SerializeField]
    private Sprite _weaponIcon;
    public Sprite WeaponIcon => _weaponIcon;

    [Tooltip("제대로 된 ID가 아니라면 오류가 납니다")]
    [SerializeField]
    private WeaponID _weaponID;
    public WeaponID WeaponID => _weaponID;

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
