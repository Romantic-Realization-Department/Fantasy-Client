using UnityEngine;

/// <summary>
/// 액티브 스킬 데이터의 추상 기반 클래스입니다.
/// 실제 런타임 효과는 사용자, 슬롯, 실행기 정보를 담은 ActiveSkillContext를 통해 실행합니다.
/// </summary>
public abstract class ActiveSkillData : SkillData
{
    [Space(40)]
    [Min(0f)]
    public float Cooldown;

    [SerializeField]
    private ActiveSkillDamageCategory damageCategory = ActiveSkillDamageCategory.None;

    [SerializeField]
    private bool usableOncePerDungeon;

    [
        SerializeField,
        Min(0f),
        Tooltip("인식 사거리 너머 추가 타격 반경 (0이면 인식 사거리 내만 공격)")
    ]
    private float extensionRange;

    public ActiveSkillDamageCategory DamageCategory => damageCategory;
    public virtual bool UsableOncePerDungeon => usableOncePerDungeon;
    public float ExtensionRange => extensionRange;

    /// <summary>
    /// 기존 테스트/에디터 호출 호환용 메서드입니다.
    /// 실제 전투에서는 TryUseSkill(ActiveSkillContext)를 사용합니다.
    /// </summary>
    public override void UseSkill() { }

    public bool TryUseSkill(ActiveSkillContext context)
    {
        if (!CanUseSkill(context))
            return false;

        UseSkill(context);
        return true;
    }

    protected virtual bool CanUseSkill(ActiveSkillContext context)
    {
        return context.Caster != null && context.Executor != null;
    }

    protected virtual void UseSkill(ActiveSkillContext context)
    {
        UseSkill();
    }
}
