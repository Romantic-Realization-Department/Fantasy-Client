using UnityEngine;

[CreateAssetMenu(
    fileName = "ActiveSkillDamagePassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Active Skill Damage"
)]
public sealed class ActiveSkillDamagePassiveSkill : PassiveSkillData, IActiveSkillDamageModifier
{
    [SerializeField, Min(0f)]
    private float activeSkillDamageBonusRate;

    public float ActiveSkillDamageBonusRate => activeSkillDamageBonusRate;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }
}
