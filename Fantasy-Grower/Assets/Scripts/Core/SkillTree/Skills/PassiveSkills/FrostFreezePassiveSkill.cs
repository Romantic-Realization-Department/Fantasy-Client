using UnityEngine;

[CreateAssetMenu(
    fileName = "FrostFreezePassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Frost Freeze"
)]
public sealed class FrostFreezePassiveSkill : PassiveSkillData, IFrostFreezeModifier
{
    [SerializeField, Min(1)]
    private int requiredFrostStacks = 3;

    [SerializeField, Min(0f)]
    private float freezeDuration = 1f;

    [SerializeField]
    private EntityStatModifier freezeModifier;

    public int RequiredFrostStacks => requiredFrostStacks;
    public float FreezeDuration => freezeDuration;
    public EntityStatModifier FreezeModifier => freezeModifier;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }
}
