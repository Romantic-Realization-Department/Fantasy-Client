using UnityEngine;

[CreateAssetMenu(
    fileName = "CooldownReductionPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Cooldown Reduction"
)]
public sealed class CooldownReductionPassiveSkill : PassiveSkillData, IActiveSkillCooldownModifier
{
    [SerializeField, Range(0f, 1f)]
    private float cooldownReductionRate;

    public float CooldownReductionRate => cooldownReductionRate;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }
}
