using UnityEngine;

/// <summary>테스트용 패시브 스킬. SerializeField 보너스값을 EntityStatModifier에 누산한다.</summary>
[CreateAssetMenu(
    fileName = "TestPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Test/PassiveSkill"
)]
public class TestPassiveSkill : PassiveSkillData
{
    [SerializeField]
    private int bonusAttackPower;

    [SerializeField]
    private int bonusHp;

    [SerializeField]
    private float bonusHpRecovery;

    [SerializeField]
    private float bonusCriticalPercentage;

    [SerializeField]
    private float bonusAttackSpeed;

    public override void ApplyPassive(ref EntityStatModifier modifier)
    {
        modifier.BonusAttackPower += bonusAttackPower;
        modifier.BonusHp += bonusHp;
        modifier.BonusHpRecovery += bonusHpRecovery;
        modifier.BonusCriticalPercentage += bonusCriticalPercentage;
        modifier.BonusAttackSpeed += bonusAttackSpeed;
    }
}
