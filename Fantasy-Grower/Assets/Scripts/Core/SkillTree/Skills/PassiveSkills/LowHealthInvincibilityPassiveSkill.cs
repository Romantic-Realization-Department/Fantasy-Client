using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "LowHealthInvincibilityPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Low Health Invincibility"
)]
public sealed class LowHealthInvincibilityPassiveSkill : PassiveSkillData
{
    [SerializeField, Range(0f, 1f)]
    private float healthThreshold = 0.01f;

    [SerializeField, Min(0f)]
    private float duration = 10f;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }

    public override PassiveSkillRuntime CreateRuntime(PassiveSkillRuntimeContext context)
    {
        return new Runtime(context, healthThreshold, duration);
    }

    private sealed class Runtime : PassiveSkillRuntime
    {
        private readonly float healthThreshold;
        private readonly float duration;
        private bool isInvincible;
        private bool canTrigger = true;
        private Coroutine removeCoroutine;

        public Runtime(PassiveSkillRuntimeContext context, float healthThreshold, float duration)
            : base(context)
        {
            this.healthThreshold = healthThreshold;
            this.duration = duration;

            if (Context.Owner != null)
            {
                Context.Owner.OnUpdated += Refresh;
                Context.Owner.OnBeforeDamageTaken += HandleBeforeDamageTaken;
            }
        }

        public override void Dispose()
        {
            if (Context.Owner != null)
            {
                Context.Owner.OnUpdated -= Refresh;
                Context.Owner.OnBeforeDamageTaken -= HandleBeforeDamageTaken;
            }

            StopRemoveCoroutine();
            isInvincible = false;
        }

        private void Refresh()
        {
            if (Context.Owner == null || Context.Owner.MaxHp <= 0f)
                return;

            float healthRatio = Context.Owner.Hp / Context.Owner.MaxHp;
            if (healthRatio > healthThreshold)
                canTrigger = true;

            if (!canTrigger || isInvincible || healthRatio > healthThreshold)
                return;

            Activate();
        }

        private void HandleBeforeDamageTaken(IncomingDamageContext damageContext)
        {
            if (damageContext == null || damageContext.IsCancelled)
                return;

            if (isInvincible)
            {
                damageContext.Cancel();
                return;
            }

            if (!canTrigger || Context.Owner == null || Context.Owner.MaxHp <= 0f)
                return;

            float thresholdHp = Context.Owner.MaxHp * healthThreshold;
            float predictedHp = Context.Owner.Hp - damageContext.Damage;
            if (predictedHp > thresholdHp)
                return;

            Activate();
            if (isInvincible)
                damageContext.Damage = Mathf.Max(0f, Context.Owner.Hp - thresholdHp);
        }

        private void Activate()
        {
            if (duration <= 0f || Context.SkillTreeComponent == null)
                return;

            canTrigger = false;
            isInvincible = true;
            removeCoroutine = Context.SkillTreeComponent.StartCoroutine(RemoveAfterDuration());
        }

        private IEnumerator RemoveAfterDuration()
        {
            yield return YieldInstructionCache.WaitForSeconds(duration);
            removeCoroutine = null;
            isInvincible = false;
        }

        private void StopRemoveCoroutine()
        {
            if (removeCoroutine == null || Context.SkillTreeComponent == null)
                return;

            Context.SkillTreeComponent.StopCoroutine(removeCoroutine);
            removeCoroutine = null;
        }
    }
}
