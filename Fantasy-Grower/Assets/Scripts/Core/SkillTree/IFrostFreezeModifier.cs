public interface IFrostFreezeModifier
{
    int RequiredFrostStacks { get; }
    float FreezeDuration { get; }
    EntityStatModifier FreezeModifier { get; }
}
