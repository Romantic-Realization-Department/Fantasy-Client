using UnityEngine;

[CreateAssetMenu(
    fileName = "EndureActiveSkill",
    menuName = "ScriptableObjects/SkillTree/Active/Warrior Endure"
)]
public sealed class EndureActiveSkill : ActiveSkillData
{
    [SerializeField, Min(0f)]
    private float damageReductionPerEnemy = 0.3f;

    [SerializeField, Min(0f)]
    private float duration = 5f;

    protected override bool CanUseSkill(ActiveSkillContext context)
    {
        return base.CanUseSkill(context) && duration > 0f;
    }

    protected override void UseSkill(ActiveSkillContext context)
    {
        EntityStatModifier modifier = EntityStatModifier.Zero;
        modifier.BonusDamageReduction =
            WaveController.CurrentActiveEnemyCount * damageReductionPerEnemy;
        context.Executor.ApplyTemporaryModifier(modifier, duration);
    }
}
