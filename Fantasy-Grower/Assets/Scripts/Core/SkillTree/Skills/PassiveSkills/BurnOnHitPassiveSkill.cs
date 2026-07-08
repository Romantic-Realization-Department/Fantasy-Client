using UnityEngine;

[CreateAssetMenu(
    fileName = "BurnOnHitPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Burn On Hit"
)]
public sealed class BurnOnHitPassiveSkill : PassiveSkillData
{
    [SerializeField, Min(0f)]
    private float damagePerSecond;

    [SerializeField, Min(0f)]
    private float attackPowerDamageRatePerSecond;

    [SerializeField, Min(0f)]
    private float targetMaxHpDamageRatePerSecond;

    [SerializeField, Min(0f)]
    private float duration = 3f;

    [SerializeField, Min(1)]
    private int maxStacks = 5;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }

    public override PassiveSkillRuntime CreateRuntime(PassiveSkillRuntimeContext context)
    {
        return new Runtime(
            context,
            damagePerSecond,
            attackPowerDamageRatePerSecond,
            targetMaxHpDamageRatePerSecond,
            duration,
            maxStacks
        );
    }

    private sealed class Runtime : PassiveSkillRuntime
    {
        private readonly float damagePerSecond;
        private readonly float attackPowerDamageRatePerSecond;
        private readonly float targetMaxHpDamageRatePerSecond;
        private readonly float duration;
        private readonly int maxStacks;

        public Runtime(
            PassiveSkillRuntimeContext context,
            float damagePerSecond,
            float attackPowerDamageRatePerSecond,
            float targetMaxHpDamageRatePerSecond,
            float duration,
            int maxStacks
        )
            : base(context)
        {
            this.damagePerSecond = damagePerSecond;
            this.attackPowerDamageRatePerSecond = attackPowerDamageRatePerSecond;
            this.targetMaxHpDamageRatePerSecond = targetMaxHpDamageRatePerSecond;
            this.duration = duration;
            this.maxStacks = maxStacks;

            if (Context.Owner != null)
                Context.Owner.OnDamageDealt += HandleDamageDealt;
        }

        public override void Dispose()
        {
            if (Context.Owner != null)
                Context.Owner.OnDamageDealt -= HandleDamageDealt;
        }

        private void HandleDamageDealt(Entity target, float _)
        {
            if (target == null || Context.Owner == null)
                return;

            float finalDamagePerSecond =
                damagePerSecond + Context.Owner.AttackPower * attackPowerDamageRatePerSecond;
            float finalTargetMaxHpDamageRatePerSecond = targetMaxHpDamageRatePerSecond;
            if (Context.SkillTreeComponent != null)
            {
                finalDamagePerSecond *= Context.SkillTreeComponent.GetOutgoingDamageMultiplier();
                finalTargetMaxHpDamageRatePerSecond *=
                    Context.SkillTreeComponent.GetOutgoingDamageMultiplier();
            }

            StatusEffectController controller = StatusEffectController.GetOrAdd(target);
            if (controller == null)
                return;

            bool allowsBurnStacking =
                Context.SkillTreeComponent != null
                && Context.SkillTreeComponent.AllowsBurnStacking();
            controller.ApplyBurn(
                Context.Owner,
                finalDamagePerSecond,
                finalTargetMaxHpDamageRatePerSecond,
                duration,
                allowsBurnStacking,
                maxStacks
            );
        }
    }
}
