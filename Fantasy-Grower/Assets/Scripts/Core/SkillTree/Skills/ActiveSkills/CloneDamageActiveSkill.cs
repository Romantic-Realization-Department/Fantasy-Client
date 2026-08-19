using System.Collections;
using UnityEngine;

[CreateAssetMenu(
    fileName = "CloneDamageActiveSkill",
    menuName = "ScriptableObjects/SkillTree/Active/Warrior Clone Damage"
)]
public sealed class CloneDamageActiveSkill : ActiveSkillData
{
    [SerializeField, Min(0f)]
    private float duration = 10f;

    [SerializeField, Min(0f)]
    private float copiedDamageRate = 0.5f;

    private sealed class Runtime
    {
        private readonly ActiveSkillContext context;
        private readonly float duration;
        private readonly float copiedDamageRate;
        private bool isDisposed;

        public Runtime(ActiveSkillContext context, float duration, float copiedDamageRate)
        {
            this.context = context;
            this.duration = duration;
            this.copiedDamageRate = copiedDamageRate;
        }

        public void Start()
        {
            context.Caster.OnDamageDealt += HandleDamageDealt;
            context.Executor.RegisterRuntimeCleanup(Dispose);
            context.Executor.StartCoroutine(DisposeAfterDuration());
        }

        private void HandleDamageDealt(Entity target, float damage)
        {
            if (target == null || target.Hp <= 0f || damage <= 0f)
                return;

            target.TakeDamage(damage * copiedDamageRate, context.Caster);
        }

        private IEnumerator DisposeAfterDuration()
        {
            yield return YieldInstructionCache.WaitForSeconds(duration);

            Dispose();
        }

        private void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            if (context.Caster != null)
                context.Caster.OnDamageDealt -= HandleDamageDealt;

            if (context.Executor != null)
                context.Executor.UnregisterRuntimeCleanup(Dispose);
        }
    }

    protected override bool CanUseSkill(ActiveSkillContext context)
    {
        return base.CanUseSkill(context) && duration > 0f && copiedDamageRate > 0f;
    }

    protected override void UseSkill(ActiveSkillContext context)
    {
        new Runtime(context, duration, copiedDamageRate).Start();
    }
}
