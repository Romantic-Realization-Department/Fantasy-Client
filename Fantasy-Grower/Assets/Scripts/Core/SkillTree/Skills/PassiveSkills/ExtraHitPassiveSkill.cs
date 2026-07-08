using UnityEngine;

[CreateAssetMenu(
    fileName = "ExtraHitPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Extra Hit"
)]
public sealed class ExtraHitPassiveSkill : PassiveSkillData
{
    [SerializeField, Range(0f, 1f)]
    private float triggerChance = 1f;

    [SerializeField, Min(0f)]
    private float damageRate = 1f;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }

    public override PassiveSkillRuntime CreateRuntime(PassiveSkillRuntimeContext context)
    {
        return new Runtime(context, triggerChance, damageRate);
    }

    private sealed class Runtime : PassiveSkillRuntime
    {
        private readonly float triggerChance;
        private readonly float damageRate;

        public Runtime(PassiveSkillRuntimeContext context, float triggerChance, float damageRate)
            : base(context)
        {
            this.triggerChance = triggerChance;
            this.damageRate = damageRate;

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
            if (target == null || target.Hp <= 0f || damage <= 0f)
                return;

            if (Random.value > triggerChance)
                return;

            target.TakeDamage(damage * damageRate, Context.Owner);
        }
    }
}
