using UnityEngine;

[CreateAssetMenu(
    fileName = "StatModifierPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Stat Modifier"
)]
public sealed class StatModifierPassiveSkill : PassiveSkillData
{
    [SerializeField]
    private EntityStatModifier modifier;

    public override void ApplyPassive(ref EntityStatModifier targetModifier)
    {
        targetModifier += modifier;
    }
}
