using UnityEngine;

[CreateAssetMenu(
    fileName = "AwakeningActiveSkill",
    menuName = "ScriptableObjects/SkillTree/Active/Warrior Awakening"
)]
public sealed class AwakeningActiveSkill : ActiveSkillData
{
    [SerializeField, Min(0f)]
    private float duration = 20f;

    [SerializeField]
    private EntityStatModifier awakeningModifier = new()
    {
        BonusOutgoingDamageRate = 0.2f,
        BonusCriticalPercentage = 50f,
        BonusAttackSpeed = 0.5f,
        BonusDamageReduction = 0.1f,
    };

    public override bool UsableOncePerDungeon => true;

    protected override bool CanUseSkill(ActiveSkillContext context)
    {
        return base.CanUseSkill(context) && duration > 0f;
    }

    protected override void UseSkill(ActiveSkillContext context)
    {
        context.Executor.ApplyTemporaryModifier(awakeningModifier, duration);
    }
}
