using UnityEngine;

[CreateAssetMenu(
    fileName = "LowHealthRecoveryPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Low Health Recovery"
)]
public sealed class LowHealthRecoveryPassiveSkill : PassiveSkillData
{
    [SerializeField, Range(0f, 1f)]
    private float healthThreshold = 0.5f;

    [SerializeField]
    private EntityStatModifier recoveryModifier;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }

    public override PassiveSkillRuntime CreateRuntime(PassiveSkillRuntimeContext context)
    {
        return new Runtime(context, healthThreshold, recoveryModifier);
    }

    private sealed class Runtime : PassiveSkillRuntime
    {
        private readonly float healthThreshold;
        private readonly EntityStatModifier recoveryModifier;
        private EntityStatModifierHandle modifierHandle;
        private bool isApplied;

        public Runtime(
            PassiveSkillRuntimeContext context,
            float healthThreshold,
            EntityStatModifier recoveryModifier
        )
            : base(context)
        {
            this.healthThreshold = healthThreshold;
            this.recoveryModifier = recoveryModifier;
            Context.Owner.OnUpdated += Refresh;
            Refresh();
        }

        public override void Dispose()
        {
            if (Context.Owner != null)
                Context.Owner.OnUpdated -= Refresh;

            if (Context.Owner != null && modifierHandle.IsValid)
                Context.Owner.RemoveStatModifier(modifierHandle);

            modifierHandle = default;
        }

        private void Refresh()
        {
            if (Context.Owner == null || Context.Owner.MaxHp <= 0f)
                return;

            bool shouldApply = Context.Owner.Hp / Context.Owner.MaxHp <= healthThreshold;
            if (shouldApply == isApplied)
                return;

            isApplied = shouldApply;
            if (isApplied)
            {
                modifierHandle = Context.Owner.ApplyStatModifier(recoveryModifier);
            }
            else if (modifierHandle.IsValid)
            {
                Context.Owner.RemoveStatModifier(modifierHandle);
                modifierHandle = default;
            }
        }
    }
}
