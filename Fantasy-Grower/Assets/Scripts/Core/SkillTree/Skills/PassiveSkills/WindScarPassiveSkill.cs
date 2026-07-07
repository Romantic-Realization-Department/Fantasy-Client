using UnityEngine;

[CreateAssetMenu(
    fileName = "WindScarPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Wind Scar"
)]
public sealed class WindScarPassiveSkill : PassiveSkillData
{
    [SerializeField, Min(1)]
    private int requiredHits = 5;

    [SerializeField, Min(0f)]
    private float damageRate = 1f;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }

    public override PassiveSkillRuntime CreateRuntime(PassiveSkillRuntimeContext context)
    {
        return new Runtime(context, requiredHits, damageRate);
    }

    private sealed class Runtime : PassiveSkillRuntime
    {
        private readonly int requiredHits;
        private readonly float damageRate;
        private int hitCount;

        public Runtime(PassiveSkillRuntimeContext context, int requiredHits, float damageRate)
            : base(context)
        {
            this.requiredHits = Mathf.Max(1, requiredHits);
            this.damageRate = damageRate;

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

            hitCount++;
            if (hitCount < requiredHits)
                return;

            hitCount = 0;
            float damage = Context.Owner.AttackPower * damageRate;
            if (Context.SkillTreeComponent != null)
                damage *= Context.SkillTreeComponent.GetOutgoingDamageMultiplier();

            target.TakeDamage(damage, Context.Owner);

            if (Context.SkillTreeComponent != null)
                Context.SkillTreeComponent.NotifyPassiveTriggered(PassiveTriggerType.WindScar);
        }
    }
}
