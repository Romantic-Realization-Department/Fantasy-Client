using UnityEngine;

[CreateAssetMenu(
    fileName = "FrostOnHitPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Frost On Hit"
)]
public sealed class FrostOnHitPassiveSkill : PassiveSkillData
{
    [SerializeField]
    private EntityStatModifier frostModifier;

    [SerializeField, Min(0f)]
    private float duration = 3f;

    [SerializeField, Min(1)]
    private int maxStacks = 5;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }

    public override PassiveSkillRuntime CreateRuntime(PassiveSkillRuntimeContext context)
    {
        return new Runtime(context, frostModifier, duration, maxStacks);
    }

    private sealed class Runtime : PassiveSkillRuntime
    {
        private readonly EntityStatModifier frostModifier;
        private readonly float duration;
        private readonly int maxStacks;

        public Runtime(
            PassiveSkillRuntimeContext context,
            EntityStatModifier frostModifier,
            float duration,
            int maxStacks
        )
            : base(context)
        {
            this.frostModifier = frostModifier;
            this.duration = duration;
            this.maxStacks = maxStacks;

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
            if (target == null)
                return;

            StatusEffectController controller = StatusEffectController.GetOrAdd(target);
            if (controller == null)
                return;

            float frostEffectMultiplier =
                Context.SkillTreeComponent != null
                    ? Context.SkillTreeComponent.GetFrostEffectMultiplier()
                    : 1f;
            EntityStatModifier scaledModifier = EntityStatModifierUtility.Scale(
                frostModifier,
                frostEffectMultiplier
            );
            controller.ApplyModifierEffect(
                StatusEffectType.Frost,
                Context.Owner,
                scaledModifier,
                duration,
                true,
                maxStacks
            );

            if (
                Context.SkillTreeComponent != null
                && Context.SkillTreeComponent.TryGetFrostFreezeRule(
                    out int requiredStacks,
                    out float freezeDuration,
                    out EntityStatModifier freezeModifier
                )
                && controller.GetStackCount(StatusEffectType.Frost) >= requiredStacks
            )
            {
                controller.RemoveAll(StatusEffectType.Frost);
                controller.ApplyModifierEffect(
                    StatusEffectType.Freeze,
                    Context.Owner,
                    freezeModifier,
                    freezeDuration,
                    false,
                    1
                );
            }
        }
    }
}
