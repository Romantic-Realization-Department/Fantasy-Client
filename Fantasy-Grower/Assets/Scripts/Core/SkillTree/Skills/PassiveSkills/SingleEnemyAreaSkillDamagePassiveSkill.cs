using UnityEngine;

[CreateAssetMenu(
    fileName = "SingleEnemyAreaSkillDamagePassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Single Enemy Area Skill Damage"
)]
public sealed class SingleEnemyAreaSkillDamagePassiveSkill
    : PassiveSkillData,
        ISingleEnemyAreaSkillDamageModifier
{
    [SerializeField, Min(0f)]
    private float singleEnemyAreaSkillDamageBonusRate;

    public float SingleEnemyAreaSkillDamageBonusRate => singleEnemyAreaSkillDamageBonusRate;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }
}
