using UnityEngine;

[CreateAssetMenu(
    fileName = "FrostEffectBonusPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Frost Effect Bonus"
)]
public sealed class FrostEffectBonusPassiveSkill : PassiveSkillData, IFrostEffectBonusModifier
{
    [SerializeField, Min(0f)]
    private float frostEffectBonusRate;

    public float FrostEffectBonusRate => frostEffectBonusRate;

    public override void ApplyPassive(ref EntityStatModifier modifier) { }
}
