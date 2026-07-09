using UnityEngine;

[CreateAssetMenu(
    fileName = "StatBuffActiveSkill",
    menuName = "ScriptableObjects/SkillTree/Active/Stat Buff"
)]
public sealed class StatBuffActiveSkill : ActiveSkillData
{
    [SerializeField]
    private EntityStatModifier modifier;

    [SerializeField, Min(0f)]
    private float duration = 5f;

    protected override bool CanUseSkill(ActiveSkillContext context)
    {
        return base.CanUseSkill(context) && duration > 0f;
    }

    protected override void UseSkill(ActiveSkillContext context)
    {
        context.Executor.ApplyTemporaryModifier(modifier, duration);
    }
}
