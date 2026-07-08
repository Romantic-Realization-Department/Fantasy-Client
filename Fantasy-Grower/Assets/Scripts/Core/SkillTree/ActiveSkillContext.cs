/// <summary>
/// 액티브 스킬이 실행될 때 필요한 런타임 정보를 전달합니다.
/// ScriptableObject인 스킬 데이터가 Entity 핸들 같은 런타임 상태를 직접 보관하지 않도록 분리합니다.
/// </summary>
public readonly struct ActiveSkillContext
{
    public ActiveSkillContext(
        ActiveSkillExecutor executor,
        SkillTreeComponent skillTreeComponent,
        Entity caster,
        ActiveSkillData skill,
        int slotIndex
    )
    {
        Executor = executor;
        SkillTreeComponent = skillTreeComponent;
        Caster = caster;
        Skill = skill;
        SlotIndex = slotIndex;
    }

    public ActiveSkillExecutor Executor { get; }
    public SkillTreeComponent SkillTreeComponent { get; }
    public Entity Caster { get; }
    public ActiveSkillData Skill { get; }
    public int SlotIndex { get; }

    public bool TryCollectTargets(
        ActiveSkillTargetMode targetMode,
        int maxTargets,
        System.Collections.Generic.List<Entity> results,
        float extensionRange = 0f
    )
    {
        if (Executor == null)
            return false;

        return Executor.TryCollectTargets(targetMode, maxTargets, results, extensionRange);
    }

    public System.Collections.Generic.IReadOnlyList<Entity> CollectTargets(
        ActiveSkillTargetMode targetMode,
        int maxTargets,
        float extensionRange = 0f
    )
    {
        if (Executor == null)
            return System.Array.Empty<Entity>();

        return Executor.CollectTargets(targetMode, maxTargets, extensionRange);
    }

    public float GetModifiedDamage(float baseDamage)
    {
        if (Executor == null)
            return baseDamage;

        float multiplier = Caster != null ? Caster.OutgoingDamageMultiplier : 1f;
        return baseDamage * multiplier * Executor.GetActiveSkillDamageMultiplier(Skill);
    }
}
