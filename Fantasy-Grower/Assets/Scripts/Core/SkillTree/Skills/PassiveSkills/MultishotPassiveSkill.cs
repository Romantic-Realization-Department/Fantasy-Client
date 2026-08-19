using UnityEngine;

[CreateAssetMenu(
    fileName = "MultishotPassiveSkill",
    menuName = "ScriptableObjects/SkillTree/Passive/Multishot"
)]
public sealed class MultishotPassiveSkill : PassiveSkillData, IBasicAttackDamageModifier
{
    [SerializeField, Min(0f), Tooltip("일반 공격의 개별 화살 데미지 배율 (예: 0.75 = 75%)")]
    private float basicAttackDamageMultiplier = 0.75f;

    [SerializeField, Min(1), Tooltip("추가되는 일반 공격 대상 수")]
    private int bonusTargets = 1;

    public float BasicAttackDamageMultiplier => basicAttackDamageMultiplier;

    public override void ApplyPassive(ref EntityStatModifier modifier)
    {
        modifier.BonusBasicAttackTargetCount += bonusTargets;
    }
}
