using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "DamageReflectionActiveSkill",
    menuName = "ScriptableObjects/SkillTree/Active/Warrior Damage Reflection"
)]
public sealed class DamageReflectionActiveSkill : ActiveSkillData
{
    [SerializeField, Min(0f)]
    private float duration = 3f;

    [SerializeField, Min(0f)]
    private float reflectedDamageMultiplier = 2f;

    private sealed class Runtime
    {
        private readonly ActiveSkillContext context;
        private readonly float duration;
        private readonly float reflectedDamageMultiplier;
        private readonly List<Entity> targets = new();
        private float accumulatedDamage;
        private bool isDisposed;

        public Runtime(ActiveSkillContext context, float duration, float reflectedDamageMultiplier)
        {
            this.context = context;
            this.duration = duration;
            this.reflectedDamageMultiplier = reflectedDamageMultiplier;
        }

        public void Start()
        {
            context.Caster.OnDamageTaken += HandleDamageTaken;
            context.Executor.RegisterRuntimeCleanup(Dispose);
            context.Executor.StartCoroutine(ReflectAfterDuration());
        }

        private void HandleDamageTaken(float previousHp, float currentHp)
        {
            accumulatedDamage += Mathf.Max(0f, previousHp - currentHp);
        }

        private IEnumerator ReflectAfterDuration()
        {
            yield return YieldInstructionCache.WaitForSeconds(duration);

            Dispose();
            if (context.Caster == null || accumulatedDamage <= 0f)
                yield break;

            if (!WaveController.TryCollectActiveEnemies(targets))
                yield break;

            float damage = context.GetModifiedDamage(accumulatedDamage * reflectedDamageMultiplier);
            for (int i = 0; i < targets.Count; i++)
            {
                Entity target = targets[i];
                if (target == null || target.Hp <= 0f)
                    continue;

                float actualDamage = target.TakeDamage(damage, context.Caster);
                context.Caster.NotifyDamageDealt(target, actualDamage);
            }
        }

        private void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            if (context.Caster != null)
                context.Caster.OnDamageTaken -= HandleDamageTaken;

            if (context.Executor != null)
                context.Executor.UnregisterRuntimeCleanup(Dispose);
        }
    }

    protected override bool CanUseSkill(ActiveSkillContext context)
    {
        return base.CanUseSkill(context) && duration > 0f;
    }

    protected override void UseSkill(ActiveSkillContext context)
    {
        new Runtime(context, duration, reflectedDamageMultiplier).Start();
    }
}
