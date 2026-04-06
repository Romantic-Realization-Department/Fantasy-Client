using UnityEngine;

/// <summary>
/// 패시브 스킬 데이터의 추상 기반 클래스.
/// ApplyPassive()에서 EntityStatModifier를 수정하여 스탯 보정값을 정의한다.
/// UseSkill()은 패시브 스킬에서 호출되지 않으므로 빈 구현으로 봉인된다.
/// </summary>
public abstract class PassiveSkillData : SkillData
{
    public override void UseSkill() { }

    /// <summary>
    /// 패시브 효과를 스탯 수정자에 누산한다.
    /// SkillTreeComponent.RecalculatePassives()에서 일괄 호출된다.
    /// </summary>
    public abstract void ApplyPassive(ref EntityStatModifier modifier);
}
