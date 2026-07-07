using UnityEngine;

[CreateAssetMenu(
    fileName = "KillStackAttackPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Kill Stack Attack"
)]
public sealed class KillStackAttackPassiveSkill : PassiveSkillData
{
    [SerializeField]
    private EntityStatModifier modifierPerStack;

    [SerializeField, Min(0)]
    private int maxStack;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }

    public override PassiveSkillRuntime CreateRuntime(PassiveSkillRuntimeContext context)
    {
        return new Runtime(context, modifierPerStack, maxStack);
    }

    private sealed class Runtime : PassiveSkillRuntime
    {
        private readonly EntityStatModifier modifierPerStack;
        private readonly int maxStack;
        private EntityStatModifierHandle modifierHandle;
        private int currentStack;

        public Runtime(
            PassiveSkillRuntimeContext context,
            EntityStatModifier modifierPerStack,
            int maxStack
        )
            : base(context)
        {
            this.modifierPerStack = modifierPerStack;
            this.maxStack = maxStack;
            WaveController.OnEnemyDiedGlobal += HandleEnemyDied;
        }

        public override void Dispose()
        {
            WaveController.OnEnemyDiedGlobal -= HandleEnemyDied;

            if (Context.Owner != null && modifierHandle.IsValid)
                Context.Owner.RemoveStatModifier(modifierHandle);

            modifierHandle = default;
        }

        private void HandleEnemyDied(Enemy _)
        {
            if (Context.Owner == null || Context.Owner.Hp <= 0f)
                return;

            if (maxStack > 0 && currentStack >= maxStack)
                return;

            currentStack++;
            EntityStatModifier modifier = ScaleModifier(modifierPerStack, currentStack);

            if (modifierHandle.IsValid)
            {
                if (!Context.Owner.UpdateStatModifier(modifierHandle, modifier))
                    modifierHandle = Context.Owner.ApplyStatModifier(modifier);
            }
            else
            {
                modifierHandle = Context.Owner.ApplyStatModifier(modifier);
            }
        }

        private static EntityStatModifier ScaleModifier(EntityStatModifier modifier, int stack)
        {
            return new EntityStatModifier
            {
                BonusHp = modifier.BonusHp * stack,
                BonusHpRecovery = modifier.BonusHpRecovery * stack,
                BonusDamageReduction = modifier.BonusDamageReduction * stack,
                BonusAttackPower = modifier.BonusAttackPower * stack,
                BonusAttackSpeed = modifier.BonusAttackSpeed * stack,
                BonusCriticalPercentage = modifier.BonusCriticalPercentage * stack,
                BonusHpRate = modifier.BonusHpRate * stack,
                BonusHpRecoveryRate = modifier.BonusHpRecoveryRate * stack,
                BonusDamageReductionRate = modifier.BonusDamageReductionRate * stack,
                BonusAttackPowerRate = modifier.BonusAttackPowerRate * stack,
                BonusAttackSpeedRate = modifier.BonusAttackSpeedRate * stack,
                BonusCriticalPercentageRate = modifier.BonusCriticalPercentageRate * stack,
            };
        }
    }
}
