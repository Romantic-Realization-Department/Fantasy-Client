using UnityEngine;

[CreateAssetMenu(
    fileName = "BurnTargetBonusDamagePassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Burn Target Bonus Damage"
)]
public sealed class BurnTargetBonusDamagePassiveSkill : PassiveSkillData
{
    [SerializeField, Min(0f)]
    private float bonusDamageRate;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }

    public override PassiveSkillRuntime CreateRuntime(PassiveSkillRuntimeContext context)
    {
        return new Runtime(context, bonusDamageRate);
    }

    private sealed class Runtime : PassiveSkillRuntime
    {
        private readonly float bonusDamageRate;

        public Runtime(PassiveSkillRuntimeContext context, float bonusDamageRate)
            : base(context)
        {
            this.bonusDamageRate = bonusDamageRate;

            if (Context.Owner != null)
                Context.Owner.OnDamageDealt += HandleDamageDealt;
        }

        public override void Dispose()
        {
            if (Context.Owner != null)
                Context.Owner.OnDamageDealt -= HandleDamageDealt;
        }

        private void HandleDamageDealt(Entity target, float damage)
        {
            if (target == null || damage <= 0f)
                return;

            if (
                target.TryGetComponent(out StatusEffectController controller)
                && controller.HasEffect(StatusEffectType.Burn)
            )
            {
                target.TakeDamage(damage * bonusDamageRate, Context.Owner);
            }
        }
    }
}
