using UnityEngine;

/// <summary>
/// 일반 전투 플레이어를 나타내는 공통 기반 클래스입니다.
/// 실제 공격 방식은 EntityAttackBehaviour 컴포넌트 조합으로 결정합니다.
/// </summary>
public class CombatPlayer : Player
{
    [SerializeField, Header("공격 방식")]
    private EntityAttackBehaviour attackBehaviour;

    protected override void Awake()
    {
        base.Awake();
        ResolveAttackBehaviour();
    }

    public override void Attack()
    {
        // 프로젝트 규칙에 따라 피해 판정을 먼저 완료한 뒤 공격 애니메이션을 실행합니다.
        if (attackBehaviour == null || !attackBehaviour.TryAttack(this))
            return;

        base.Attack();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        ResolveAttackBehaviour();

        if (attackBehaviour == null)
        {
            Debug.LogError("[CombatPlayer] EntityAttackBehaviour를 할당해주세요.", this);
        }
    }

    private void ResolveAttackBehaviour()
    {
        if (attackBehaviour == null)
            attackBehaviour = GetComponent<EntityAttackBehaviour>();
    }
}
