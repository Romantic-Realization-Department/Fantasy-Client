using UnityEngine;

[CreateAssetMenu(
    fileName = "LowHealthDamageHealPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Low Health Damage Heal"
)]
public sealed class LowHealthDamageHealPassiveSkill : PassiveSkillData
{
    [SerializeField, Range(0f, 1f)]
    private float healthThreshold = 0.5f;

    [SerializeField, Range(0f, 1f)]
    private float damageHealRate;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }

    public override PassiveSkillRuntime CreateRuntime(PassiveSkillRuntimeContext context)
    {
        return new Runtime(context, healthThreshold, damageHealRate);
    }

    private sealed class Runtime : PassiveSkillRuntime
    {
        private readonly float healthThreshold;
        private readonly float damageHealRate;

        public Runtime(
            PassiveSkillRuntimeContext context,
            float healthThreshold,
            float damageHealRate
        )
            : base(context)
        {
            this.healthThreshold = healthThreshold;
            this.damageHealRate = damageHealRate;
            Context.Owner.OnDamageDealt += HandleDamageDealt;
        }

        public override void Dispose()
        {
            if (Context.Owner != null)
                Context.Owner.OnDamageDealt -= HandleDamageDealt;
        }

        private void HandleDamageDealt(Entity _, float damage)
        {
            if (Context.Owner == null || Context.Owner.MaxHp <= 0f)
                return;

            if (Context.Owner.Hp / Context.Owner.MaxHp > healthThreshold)
                return;

            Context.Owner.Heal(damage * damageHealRate);
        }
    }
}
