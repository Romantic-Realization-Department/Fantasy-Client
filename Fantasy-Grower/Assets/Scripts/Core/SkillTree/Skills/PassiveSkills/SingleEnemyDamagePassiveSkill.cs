using UnityEngine;

[CreateAssetMenu(
    fileName = "SingleEnemyDamagePassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Single Enemy Damage"
)]
public sealed class SingleEnemyDamagePassiveSkill : PassiveSkillData, ISingleEnemyDamageModifier
{
    [SerializeField, Min(0f)]
    private float singleEnemyDamageBonusRate;

    public float SingleEnemyDamageBonusRate => singleEnemyDamageBonusRate;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }
}
