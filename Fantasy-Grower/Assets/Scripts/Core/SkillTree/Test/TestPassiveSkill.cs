using UnityEngine;

/// <summary>테스트용 패시브 스킬. SerializeField 보너스값을 EntityStatModifier에 누산한다.</summary>
[CreateAssetMenu(
    fileName = "TestPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Test/PassiveSkill"
)]
public class TestPassiveSkill : PassiveSkillData
{
    [SerializeField]
    private EntityStatModifier bonusStat;

    public override void ApplyPassive(ref EntityStatModifier modifier)
    {
        modifier += bonusStat;
    }
}
