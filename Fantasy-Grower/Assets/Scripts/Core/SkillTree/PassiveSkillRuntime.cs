public readonly struct PassiveSkillRuntimeContext
{
    public PassiveSkillRuntimeContext(SkillTreeComponent skillTreeComponent, Entity owner)
    {
        SkillTreeComponent = skillTreeComponent;
        Owner = owner;
    }

    public SkillTreeComponent SkillTreeComponent { get; }
    public Entity Owner { get; }
}

public abstract class PassiveSkillRuntime
{
    protected PassiveSkillRuntime(PassiveSkillRuntimeContext context)
    {
        Context = context;
    }

    protected PassiveSkillRuntimeContext Context { get; }

    public abstract void Dispose();
}
