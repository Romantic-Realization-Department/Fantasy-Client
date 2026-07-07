using UnityEngine;

[CreateAssetMenu(
    fileName = "RiskRewardPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Risk Reward"
)]
public sealed class RiskRewardPassiveSkill : PassiveSkillData, IOutgoingDamageModifier
{
    [SerializeField, Min(0f)]
    private float outgoingDamageBonusRate = 0.5f;

    [SerializeField, Min(0f)]
    private float incomingDamageBonusRate = 0.3f;

    public float OutgoingDamageBonusRate => outgoingDamageBonusRate;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }

    public override PassiveSkillRuntime CreateRuntime(PassiveSkillRuntimeContext context)
    {
        return new Runtime(context, incomingDamageBonusRate);
    }

    private sealed class Runtime : PassiveSkillRuntime
    {
        private readonly float incomingDamageBonusRate;

        public Runtime(PassiveSkillRuntimeContext context, float incomingDamageBonusRate)
            : base(context)
        {
            this.incomingDamageBonusRate = incomingDamageBonusRate;

            if (Context.Owner != null)
                Context.Owner.OnBeforeDamageTaken += HandleBeforeDamageTaken;
        }

        public override void Dispose()
        {
            if (Context.Owner != null)
                Context.Owner.OnBeforeDamageTaken -= HandleBeforeDamageTaken;
        }

        private void HandleBeforeDamageTaken(IncomingDamageContext damageContext)
        {
            if (damageContext == null || damageContext.IsCancelled)
                return;

            damageContext.Damage *= 1f + incomingDamageBonusRate;
        }
    }
}
