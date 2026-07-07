public static class EntityStatModifierUtility
{
    public static EntityStatModifier Scale(EntityStatModifier modifier, float multiplier)
    {
        return new EntityStatModifier
        {
            BonusHp = modifier.BonusHp * multiplier,
            BonusHpRecovery = modifier.BonusHpRecovery * multiplier,
            BonusDamageReduction = modifier.BonusDamageReduction * multiplier,
            BonusAttackPower = modifier.BonusAttackPower * multiplier,
            BonusAttackSpeed = modifier.BonusAttackSpeed * multiplier,
            BonusCriticalPercentage = modifier.BonusCriticalPercentage * multiplier,
            BonusCriticalDamageRate = modifier.BonusCriticalDamageRate * multiplier,
            BonusAttackRangeRate = modifier.BonusAttackRangeRate * multiplier,
            BonusMoveSpeedRate = modifier.BonusMoveSpeedRate * multiplier,
            BonusHpRate = modifier.BonusHpRate * multiplier,
            BonusHpRecoveryRate = modifier.BonusHpRecoveryRate * multiplier,
            BonusDamageReductionRate = modifier.BonusDamageReductionRate * multiplier,
            BonusAttackPowerRate = modifier.BonusAttackPowerRate * multiplier,
            BonusAttackSpeedRate = modifier.BonusAttackSpeedRate * multiplier,
            BonusCriticalPercentageRate = modifier.BonusCriticalPercentageRate * multiplier,
        };
    }
}
