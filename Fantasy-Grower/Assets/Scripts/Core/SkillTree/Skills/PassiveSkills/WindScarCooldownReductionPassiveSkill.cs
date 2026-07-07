using UnityEngine;

[CreateAssetMenu(
    fileName = "WindScarCooldownReductionPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Wind Scar Cooldown Reduction"
)]
public sealed class WindScarCooldownReductionPassiveSkill : PassiveSkillData
{
    [SerializeField, Min(0f)]
    private float reduceCooldownSeconds = 1f;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }

    public override PassiveSkillRuntime CreateRuntime(PassiveSkillRuntimeContext context)
    {
        return new Runtime(context, reduceCooldownSeconds);
    }

    private sealed class Runtime : PassiveSkillRuntime
    {
        private readonly float reduceCooldownSeconds;
        private readonly ActiveSkillExecutor executor;

        public Runtime(PassiveSkillRuntimeContext context, float reduceCooldownSeconds)
            : base(context)
        {
            this.reduceCooldownSeconds = reduceCooldownSeconds;
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
            if (triggerType != PassiveTriggerType.WindScar || reduceCooldownSeconds <= 0f)
                return;

            if (executor != null)
                executor.ReduceAllCooldowns(reduceCooldownSeconds);
        }
    }
}
