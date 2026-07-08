public static class EntityStatModifierUtility
{
    public static bool IsZero(EntityStatModifier modifier)
    {
        return modifier.BonusHp == 0f
            && modifier.BonusHpRecovery == 0f
            && modifier.BonusDamageReduction == 0f
            && modifier.BonusAttackPower == 0f
            && modifier.BonusAttackSpeed == 0f
            && modifier.BonusCriticalPercentage == 0f
            && modifier.BonusCriticalDamageRate == 0f
            && modifier.BonusAttackRangeRate == 0f
            && modifier.BonusMoveSpeedRate == 0f
            && modifier.BonusOutgoingDamageRate == 0f
            && modifier.BonusHpRate == 0f
            && modifier.BonusHpRecoveryRate == 0f
            && modifier.BonusDamageReductionRate == 0f
            && modifier.BonusAttackPowerRate == 0f
            && modifier.BonusAttackSpeedRate == 0f
            && modifier.BonusCriticalPercentageRate == 0f;
    }

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
            BonusOutgoingDamageRate = modifier.BonusOutgoingDamageRate * multiplier,
            BonusHpRate = modifier.BonusHpRate * multiplier,
            BonusHpRecoveryRate = modifier.BonusHpRecoveryRate * multiplier,
            BonusDamageReductionRate = modifier.BonusDamageReductionRate * multiplier,
            BonusAttackPowerRate = modifier.BonusAttackPowerRate * multiplier,
            BonusAttackSpeedRate = modifier.BonusAttackSpeedRate * multiplier,
            BonusCriticalPercentageRate = modifier.BonusCriticalPercentageRate * multiplier,
        };
    }
}
