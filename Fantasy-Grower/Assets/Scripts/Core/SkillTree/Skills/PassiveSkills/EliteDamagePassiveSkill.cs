using UnityEngine;

[CreateAssetMenu(
    fileName = "EliteDamagePassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Elite Damage"
)]
public sealed class EliteDamagePassiveSkill : PassiveSkillData, IEliteDamageModifier
{
    [SerializeField, Min(0f)]
    private float eliteDamageBonusRate;

    public float EliteDamageBonusRate => eliteDamageBonusRate;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }
}
