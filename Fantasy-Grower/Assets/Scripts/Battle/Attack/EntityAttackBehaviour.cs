using UnityEngine;

/// <summary>
/// 엔티티가 공격할 때 실행할 판정 방식을 정의합니다.
/// 공격 판정이 성공한 경우에만 true를 반환하여 이후 공격 애니메이션이 실행되도록 합니다.
/// </summary>
public abstract class EntityAttackBehaviour : MonoBehaviour
{
    public abstract bool TryAttack(Entity attacker);
}
