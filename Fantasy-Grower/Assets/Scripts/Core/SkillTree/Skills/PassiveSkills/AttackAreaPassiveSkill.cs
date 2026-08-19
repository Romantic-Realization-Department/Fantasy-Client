using UnityEngine;

[CreateAssetMenu(
    fileName = "AttackAreaPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Attack Area"
)]
public sealed class AttackAreaPassiveSkill : PassiveSkillData, IAttackAreaModifier
{
    [SerializeField, Min(0f)]
    private float attackAreaBonusRate;

    public float AttackAreaBonusRate => attackAreaBonusRate;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }
}
