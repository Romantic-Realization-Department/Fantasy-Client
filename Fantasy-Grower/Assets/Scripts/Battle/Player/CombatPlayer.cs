using UnityEngine;

/// <summary>
/// 적을 탐색하여 공격하는 일반 전투 플레이어의 공통 기반 클래스입니다.
/// 채광처럼 공격 대상을 별도 방식으로 결정하는 Player에게는 AttackTargetsSensing을 강제하지 않습니다.
/// </summary>
public abstract class CombatPlayer : Player
{
    [SerializeField, Header("공격 설정")]
    protected AttackTargetsSensing targets;

    protected override void OnValidate()
    {
        if (targets == null)
        {
            Debug.LogError(
                "[CombatPlayer] Targets 필드에 AttackTargetsSensing 컴포넌트를 할당해주세요.",
                this
            );
        }

        base.OnValidate();
    }
}
