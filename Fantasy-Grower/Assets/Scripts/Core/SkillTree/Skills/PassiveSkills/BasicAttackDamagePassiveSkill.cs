using UnityEngine;

[CreateAssetMenu(
    fileName = "BasicAttackDamagePassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Basic Attack Damage"
)]
public sealed class BasicAttackDamagePassiveSkill : PassiveSkillData, IBasicAttackDamageModifier
{
    [SerializeField, Min(0f)]
    private float basicAttackDamageMultiplier = 1f;

    public float BasicAttackDamageMultiplier => basicAttackDamageMultiplier;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }
}
