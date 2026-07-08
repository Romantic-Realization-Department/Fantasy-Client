using UnityEngine;

/// <summary>
/// 스킬 트리에서 기본 평타(Tier 0)를 정의하는 데이터 클래스.
/// </summary>
[CreateAssetMenu(
    fileName = "BasicAttackSkillData",
    menuName = "ScriptableObjects/SkillTree/BasicAttack"
)]
public class BasicAttackSkillData : SkillData
{
    [SerializeField, Min(0f), Tooltip("기본 공격력 배율 (예: 1.0 = 100%)")]
    private float damageRate = 1f;
    public float DamageRate => damageRate;

    [SerializeField, Min(1), Tooltip("공격 가능한 최대 타겟 수")]
    private int maxTargets = 1;
    public int MaxTargets => maxTargets;

    [SerializeField, Tooltip("장착(해금) 시 영구적으로 증가하는 기본 공격 속도")]
    private float bonusAttackSpeed = 0f;
    public float BonusAttackSpeed => bonusAttackSpeed;

    [
        SerializeField,
        Min(0f),
        Tooltip("인식 사거리 너머 추가 타격 반경 (0이면 인식 사거리 내만 공격)")
    ]
    private float extensionRange = 0f;
    public float ExtensionRange => extensionRange;

    public override void UseSkill()
    {
        // 평타는 AutoAttackController 및 EntityAttackBehaviour에 의해 구동되므로 직접 UseSkill을 호출하지 않습니다.
    }
}
