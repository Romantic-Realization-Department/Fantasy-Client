using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "WaveOnceDamageReductionPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Wave Once Damage Reduction"
)]
public sealed class WaveOnceDamageReductionPassiveSkill : PassiveSkillData
{
    [SerializeField]
    private EntityStatModifier damageReductionModifier;

    [SerializeField, Min(0f)]
    private float duration = 10f;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }

    public override PassiveSkillRuntime CreateRuntime(PassiveSkillRuntimeContext context)
    {
        return new Runtime(context, damageReductionModifier, duration);
    }

    private sealed class Runtime : PassiveSkillRuntime
    {
        private readonly EntityStatModifier damageReductionModifier;
        private readonly float duration;
        private EntityStatModifierHandle handle;
        private Coroutine removeCoroutine;
        private bool usedInCurrentWave;

        public Runtime(
            PassiveSkillRuntimeContext context,
            EntityStatModifier damageReductionModifier,
            float duration
        )
            : base(context)
        {
            this.damageReductionModifier = damageReductionModifier;
            this.duration = duration;

            if (Context.Owner != null)
                Context.Owner.OnBeforeDamageTaken += HandleBeforeDamageTaken;

            WaveController.OnWaveStartedGlobal += ResetWaveUsage;
        }

        public override void Dispose()
        {
            if (Context.Owner != null)
                Context.Owner.OnBeforeDamageTaken -= HandleBeforeDamageTaken;

            WaveController.OnWaveStartedGlobal -= ResetWaveUsage;
            RemoveModifier();
        }

        private void HandleBeforeDamageTaken(IncomingDamageContext damageContext)
        {
            if (
                usedInCurrentWave
                || damageContext == null
                || damageContext.IsCancelled
                || Context.Owner == null
            )
            {
                return;
            }

            usedInCurrentWave = true;
            if (handle.IsValid)
                return;

            handle = Context.Owner.ApplyStatModifier(damageReductionModifier);

            if (duration > 0f)
                removeCoroutine = Context.SkillTreeComponent.StartCoroutine(RemoveAfterDuration());
        }

        private IEnumerator RemoveAfterDuration()
        {
            yield return YieldInstructionCache.WaitForSeconds(duration);
            removeCoroutine = null;
            RemoveModifier();
        }

        private void ResetWaveUsage()
        {
            usedInCurrentWave = false;
        }

        private void RemoveModifier()
        {
            if (removeCoroutine != null && Context.SkillTreeComponent != null)
            {
                Context.SkillTreeComponent.StopCoroutine(removeCoroutine);
                removeCoroutine = null;
            }

            if (Context.Owner != null && handle.IsValid)
                Context.Owner.RemoveStatModifier(handle);

            handle = default;
        }
    }
}
