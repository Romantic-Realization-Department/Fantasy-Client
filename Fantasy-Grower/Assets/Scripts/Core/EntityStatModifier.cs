[System.Serializable]
public struct EntityStatModifier
{
    public float BonusHp;
    public float BonusHpRecovery;
    public float BonusDamageReduction;
    public float BonusAttackPower;
    public float BonusAttackSpeed;
    public float BonusCriticalPercentage;

    public static EntityStatModifier operator +(EntityStatModifier a, EntityStatModifier b)
    {
        return new EntityStatModifier
        {
            BonusHp = a.BonusHp + b.BonusHp,
            BonusHpRecovery = a.BonusHpRecovery + b.BonusHpRecovery,
            BonusDamageReduction = a.BonusDamageReduction + b.BonusDamageReduction,
            BonusAttackPower = a.BonusAttackPower + b.BonusAttackPower,
            BonusAttackSpeed = a.BonusAttackSpeed + b.BonusAttackSpeed,
            BonusCriticalPercentage = a.BonusCriticalPercentage + b.BonusCriticalPercentage,
        };
    }

    public static EntityStatModifier Zero => default;
}
