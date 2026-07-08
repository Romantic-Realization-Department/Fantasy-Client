using UnityEngine;

[CreateAssetMenu(
    fileName = "CooldownResetActiveSkill",
    menuName = "ScriptableObjects/SkillTree/Active/Cooldown Reset"
)]
public sealed class CooldownResetActiveSkill : ActiveSkillData
{
    protected override void UseSkill(ActiveSkillContext context)
    {
        context.Executor.ResetCooldownsExcept(context.SlotIndex);
    }
}
