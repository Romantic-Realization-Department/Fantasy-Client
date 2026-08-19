using UnityEngine;

[CreateAssetMenu(
    fileName = "AreaActiveSkillDamagePassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Area Active Skill Damage"
)]
public sealed class AreaActiveSkillDamagePassiveSkill
    : PassiveSkillData,
        IAreaActiveSkillDamageModifier
{
    [SerializeField, Min(0f)]
    private float areaActiveSkillDamageBonusRate;

    public float AreaActiveSkillDamageBonusRate => areaActiveSkillDamageBonusRate;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }
}
