using UnityEngine;

[System.Serializable]
public struct EntityStatModifier
{
    public int BonusHp;
    public float BonusDamageReduction;
    public int BonusAttackPower;
    public float BonusAttackSpeed;
    public float BonusCriticalPercentage;

    public static EntityStatModifier operator +(EntityStatModifier a, EntityStatModifier b)
    {
        return new EntityStatModifier
        {
            BonusHp = a.BonusHp + b.BonusHp,
            BonusDamageReduction = a.BonusDamageReduction + b.BonusDamageReduction,
            BonusAttackPower = a.BonusAttackPower + b.BonusAttackPower,
            BonusAttackSpeed = a.BonusAttackSpeed + b.BonusAttackSpeed,
            BonusCriticalPercentage = a.BonusCriticalPercentage + b.BonusCriticalPercentage,
        };
    }

    public static EntityStatModifier Zero => default;
}
