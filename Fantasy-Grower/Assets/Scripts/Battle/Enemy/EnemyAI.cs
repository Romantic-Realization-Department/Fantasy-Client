using System.Collections;
using UnityEngine;

/// <summary>
/// 적의 자동 공격 AI.
/// BattleManager가 스폰 후 Initialize()와 StartAttacking()을 호출하여 동작을 시작한다.
/// </summary>
[RequireComponent(typeof(Enemy))]
public class EnemyAI : MonoBehaviour
{
    private Enemy _enemy;
    private Player _player;
    private Coroutine _attackCoroutine;

    private void Awake()
    {
        _enemy = GetComponent<Enemy>();
    }

    /// <summary>타겟 플레이어를 설정한다. BattleManager가 스폰 직후 호출한다.</summary>
    public void Initialize(Player player)
    {
        _player = player;
    }

    public void StartAttacking()
    {
        StopAttacking();
        _attackCoroutine = StartCoroutine(AttackLoop());
    }

    public void StopAttacking()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            if (_player != null && _player.Hp > 0)
            {
                // Enemy 프리팹에 AttackCollider가 있으면 Attack()으로 애니메이션+충돌 처리.
                // AttackCollider가 없는 경우 DamageCalculator로 직접 피해 적용.
                if (HasAttackCollider())
                {
                    _enemy.Attack();
                }
                else
                {
                    var (damage, _) = DamageCalculator.Calculate(
                        _enemy.AttackPower,
                        _player.DamageReduction,
                        _enemy.CriticalPercentage
                    );
                    _player.TakeDamage(damage);
                }
            }

            float interval = _enemy.AttackSpeed > 0f ? 1f / _enemy.AttackSpeed : 2f;
            yield return new WaitForSeconds(interval);
        }
    }

    private bool HasAttackCollider()
    {
        return GetComponentInChildren<AttackCollider>(includeInactive: true) != null;
    }
}
