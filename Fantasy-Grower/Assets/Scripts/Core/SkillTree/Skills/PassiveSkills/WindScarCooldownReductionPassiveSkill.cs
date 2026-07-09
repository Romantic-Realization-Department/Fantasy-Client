using UnityEngine;

[CreateAssetMenu(
    fileName = "WindScarCooldownReductionPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Wind Scar Cooldown Reduction"
)]
public sealed class WindScarCooldownReductionPassiveSkill : PassiveSkillData
{
    [
        SerializeField,
        Range(0f, 1f),
        Tooltip("칼바람 발동 시 모든 스킬 총 쿨타임 감소 비율 (예: 0.333 = 1/3 감소)")
    ]
    private float cooldownReductionRate = 0.333f;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }

    public override PassiveSkillRuntime CreateRuntime(PassiveSkillRuntimeContext context)
    {
        return new Runtime(context, cooldownReductionRate);
    }

    private sealed class Runtime : PassiveSkillRuntime
    {
        private readonly float cooldownReductionRate;
        private readonly ActiveSkillExecutor executor;

        public Runtime(PassiveSkillRuntimeContext context, float cooldownReductionRate)
            : base(context)
        {
            this.cooldownReductionRate = cooldownReductionRate;
            executor =
                context.Owner != null ? context.Owner.GetComponent<ActiveSkillExecutor>() : null;

            if (Context.SkillTreeComponent != null)
                Context.SkillTreeComponent.OnPassiveTriggered += HandlePassiveTriggered;
        }

        public override void Dispose()
        {
            if (Context.SkillTreeComponent != null)
                Context.SkillTreeComponent.OnPassiveTriggered -= HandlePassiveTriggered;
        }

        private void HandlePassiveTriggered(PassiveTriggerType triggerType)
        {
            if (triggerType != PassiveTriggerType.WindScar || cooldownReductionRate <= 0f)
                return;

            if (executor != null)
                executor.ReduceAllCooldownsPercent(cooldownReductionRate);
        }
    }
}
