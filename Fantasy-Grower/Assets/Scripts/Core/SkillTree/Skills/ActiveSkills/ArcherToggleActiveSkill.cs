using UnityEngine;

[CreateAssetMenu(
    fileName = "ArcherToggleActiveSkill",
    menuName = "ScriptableObjects/SkillTree/Active/Archer Toggle Buff"
)]
public sealed class ArcherToggleActiveSkill : ActiveSkillData
{
    [SerializeField, Tooltip("버프 활성화 시 보너스 공격 속도")]
    private float bonusAttackSpeed = 1.0f;

    [SerializeField, Tooltip("버프 활성화 시 일반 공격 추가 타겟 수")]
    private int bonusTargetCount = 99;

    [SerializeField, Tooltip("버프 활성화 시 받는 피해 증가 배율 (0.5 = 50% 증가)")]
    private float incomingDamageIncreaseRate = 0.5f;

    protected override void UseSkill(ActiveSkillContext context)
    {
        if (context.Caster == null)
            return;

        ToggleStateTracker tracker = context.Caster.GetComponent<ToggleStateTracker>();
        if (tracker == null)
        {
            // Toggle ON
            tracker = context.Caster.gameObject.AddComponent<ToggleStateTracker>();
            tracker.Initialize(
                context.Caster,
                bonusAttackSpeed,
                bonusTargetCount,
                incomingDamageIncreaseRate
            );
            Debug.Log(
                $"[부동자세] 활성화 (공속 +{bonusAttackSpeed}, 타겟 +{bonusTargetCount}, 받는피해 +{incomingDamageIncreaseRate * 100}%)"
            );
        }
        else
        {
            // Toggle OFF
            Destroy(tracker);
            Debug.Log("[부동자세] 비활성화");
        }
    }

    private sealed class ToggleStateTracker : MonoBehaviour
    {
        private Entity caster;
        private EntityStatModifierHandle modifierHandle;
        private float incomingDamageIncreaseRate;

        public void Initialize(
            Entity caster,
            float bonusAttackSpeed,
            int bonusTargetCount,
            float incomingDamageIncreaseRate
        )
        {
            this.caster = caster;
            this.incomingDamageIncreaseRate = incomingDamageIncreaseRate;

            // 스탯 버프 적용 (공속 및 기본공격 타겟 수)
            EntityStatModifier modifier = EntityStatModifier.Zero;
            modifier.BonusAttackSpeed = bonusAttackSpeed;
            modifier.BonusBasicAttackTargetCount = bonusTargetCount;

            modifierHandle = caster.ApplyStatModifier(modifier);

            // 받는 피해 증가 이벤트 구독
            caster.OnBeforeDamageTaken += HandleBeforeDamageTaken;
        }

        private void HandleBeforeDamageTaken(IncomingDamageContext damageContext)
        {
            if (damageContext == null || damageContext.IsCancelled)
                return;

            damageContext.Damage *= 1f + incomingDamageIncreaseRate;
        }

        private void OnDestroy()
        {
            if (caster != null)
            {
                caster.OnBeforeDamageTaken -= HandleBeforeDamageTaken;
                if (modifierHandle.IsValid)
                {
                    caster.RemoveStatModifier(modifierHandle);
                }
            }
        }
    }
}
