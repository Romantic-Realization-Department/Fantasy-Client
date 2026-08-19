using UnityEngine;

[CreateAssetMenu(
    fileName = "SingleTargetActiveSkillDamagePassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Single Target Active Skill Damage"
)]
public sealed class SingleTargetActiveSkillDamagePassiveSkill
    : PassiveSkillData,
        ISingleTargetActiveSkillDamageModifier
{
    [SerializeField, Min(0f)]
    private float singleTargetActiveSkillDamageBonusRate;

    public float SingleTargetActiveSkillDamageBonusRate => singleTargetActiveSkillDamageBonusRate;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }
}
