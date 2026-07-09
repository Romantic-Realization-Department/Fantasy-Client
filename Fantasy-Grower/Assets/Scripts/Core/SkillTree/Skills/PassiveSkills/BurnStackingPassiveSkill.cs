using UnityEngine;

[CreateAssetMenu(
    fileName = "BurnStackingPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Burn Stacking"
)]
public sealed class BurnStackingPassiveSkill : PassiveSkillData, IBurnStackingModifier
{
    [SerializeField]
    private bool allowsBurnStacking = true;

    public bool AllowsBurnStacking => allowsBurnStacking;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }
}
