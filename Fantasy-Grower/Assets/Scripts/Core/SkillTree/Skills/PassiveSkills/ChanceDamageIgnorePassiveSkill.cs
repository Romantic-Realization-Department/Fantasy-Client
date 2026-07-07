using UnityEngine;

[CreateAssetMenu(
    fileName = "ChanceDamageIgnorePassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Chance Damage Ignore"
)]
public sealed class ChanceDamageIgnorePassiveSkill : PassiveSkillData
{
    [SerializeField, Range(0f, 1f)]
    private float ignoreChance;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }

    public override PassiveSkillRuntime CreateRuntime(PassiveSkillRuntimeContext context)
    {
        return new Runtime(context, ignoreChance);
    }

    private sealed class Runtime : PassiveSkillRuntime
    {
        private readonly float ignoreChance;

        public Runtime(PassiveSkillRuntimeContext context, float ignoreChance)
            : base(context)
        {
            this.ignoreChance = ignoreChance;

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

            if (Random.value < ignoreChance)
                damageContext.Cancel();
        }
    }
}
