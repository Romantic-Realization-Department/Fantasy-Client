using UnityEngine;

[CreateAssetMenu(
    fileName = "ArcherFocusActiveSkill",
    menuName = "ScriptableObjects/SkillTree/Active/Archer Focus"
)]
public sealed class ArcherFocusActiveSkill : ActiveSkillData
{
    [SerializeField, Tooltip("다음 공격 데미지 배율 (예: 1.5 = 1.5배)")]
    private float damageMultiplier = 1.5f;

    protected override void UseSkill(ActiveSkillContext context)
    {
        if (context.Caster == null)
            return;

        // 공격력 배율 증가 버프 적용
        EntityStatModifier modifier = EntityStatModifier.Zero;
        modifier.BonusOutgoingDamageRate = damageMultiplier - 1f;

        EntityStatModifierHandle handle = context.Caster.ApplyStatModifier(modifier);

        // 다음 공격 성공 시 버프를 해제하는 헬퍼 컴포넌트 부착
        FocusTracker tracker = context.Caster.gameObject.AddComponent<FocusTracker>();
        tracker.Initialize(context.Caster, handle);
    }

    private sealed class FocusTracker : MonoBehaviour
    {
        private Entity caster;
        private EntityStatModifierHandle modifierHandle;
        private bool isTriggered;

        public void Initialize(Entity caster, EntityStatModifierHandle modifierHandle)
        {
            this.caster = caster;
            this.modifierHandle = modifierHandle;
            caster.OnDamageDealt += HandleDamageDealt;
        }

        private void HandleDamageDealt(Entity target, float damage)
        {
            if (isTriggered)
                return;

            isTriggered = true;
            Cleanup();
            Destroy(this);
        }

        private void Cleanup()
        {
            if (caster != null)
            {
                caster.OnDamageDealt -= HandleDamageDealt;
                if (modifierHandle.IsValid)
                {
                    caster.RemoveStatModifier(modifierHandle);
                    modifierHandle = default;
                }
            }
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
