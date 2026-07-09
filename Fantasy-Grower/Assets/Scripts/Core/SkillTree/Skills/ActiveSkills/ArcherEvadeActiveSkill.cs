using UnityEngine;

[CreateAssetMenu(
    fileName = "ArcherEvadeActiveSkill",
    menuName = "ScriptableObjects/SkillTree/Active/Archer Evade Buff"
)]
public sealed class ArcherEvadeActiveSkill : ActiveSkillData
{
    [SerializeField, Min(0f), Tooltip("회피 버프 지속 시간")]
    private float duration = 10f;

    [SerializeField, Min(0f), Tooltip("회피 성공 시 반격 데미지 배율 (예: 1.2 = 120%)")]
    private float counterDamageRate = 1.2f;

    protected override void UseSkill(ActiveSkillContext context)
    {
        if (context.Caster == null)
            return;

        EvadeTracker tracker = context.Caster.GetComponent<EvadeTracker>();
        if (tracker == null)
        {
            tracker = context.Caster.gameObject.AddComponent<EvadeTracker>();
            tracker.Initialize(context.Caster, duration, counterDamageRate);
        }
        else
        {
            tracker.Refresh(duration, counterDamageRate);
        }
    }

    private sealed class EvadeTracker : MonoBehaviour
    {
        private Entity caster;
        private float counterDamageRate;
        private bool isCleanedUp;

        public void Initialize(Entity caster, float duration, float counterDamageRate)
        {
            this.caster = caster;
            this.counterDamageRate = counterDamageRate;

            caster.OnBeforeDamageTaken += HandleBeforeDamageTaken;
            Invoke(nameof(Cleanup), duration);
        }

        public void Refresh(float duration, float counterDamageRate)
        {
            this.counterDamageRate = counterDamageRate;
            CancelInvoke(nameof(Cleanup));
            Invoke(nameof(Cleanup), duration);
        }

        private void HandleBeforeDamageTaken(IncomingDamageContext damageContext)
        {
            if (damageContext == null || damageContext.IsCancelled)
                return;

            damageContext.Cancel();
            Debug.Log("[추적] 피해 회피! 반격 시작");

            if (damageContext.Attacker != null && damageContext.Attacker.Hp > 0f && caster != null)
            {
                float rawDamage = caster.AttackPower * counterDamageRate;
                if (caster.TryGetComponent(out SkillTreeComponent skillTree))
                {
                    rawDamage *= skillTree.GetOutgoingDamageMultiplier();
                }
                rawDamage *= caster.OutgoingDamageMultiplier;

                (float damage, _) = DamageCalculator.Calculate(
                    rawDamage,
                    damageContext.Attacker.DamageReduction,
                    caster.CriticalPercentage,
                    caster.CriticalDamageMultiplier
                );

                float actualDamage = damageContext.Attacker.TakeDamage(damage, caster);
                caster.NotifyDamageDealt(damageContext.Attacker, actualDamage);
            }
        }

        private void Cleanup()
        {
            if (isCleanedUp)
                return;

            isCleanedUp = true;
            if (caster != null)
            {
                caster.OnBeforeDamageTaken -= HandleBeforeDamageTaken;
            }
            Destroy(this);
        }

        private void OnDestroy()
        {
            CancelInvoke();
            if (!isCleanedUp && caster != null)
            {
                caster.OnBeforeDamageTaken -= HandleBeforeDamageTaken;
            }
        }
    }
}
