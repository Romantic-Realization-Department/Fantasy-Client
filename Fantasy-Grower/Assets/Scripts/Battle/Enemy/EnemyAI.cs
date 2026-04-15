using System.Collections;
using UnityEngine;

/// <summary>
/// 적의 자동 공격 AI.
/// BattleManager가 스폰 후 Initialize()와 StartAttacking()을 호출하여 동작을 시작한다.
/// </summary>
[RequireComponent(typeof(Enemy))]
public class EnemyAI : MonoBehaviour
{
    private Enemy enemy;
    private Player player;
    private Coroutine attackCoroutine;

    private bool hasAttackCollider => childCollider != null;

    private AttackCollider childCollider;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        childCollider = GetComponentInChildren<AttackCollider>(includeInactive: true);
        enemy.OnDied += OnEnemyDied;
    }

    private void OnDestroy()
    {
        if (enemy != null)
            enemy.OnDied -= OnEnemyDied;
    }

    private void OnEnemyDied(Entity _) => StopAttacking();

    /// <summary>타겟 플레이어를 설정한다. BattleManager가 스폰 직후 호출한다.</summary>
    public void Initialize(Player player)
    {
        this.player = player;
    }

    public void StartAttacking()
    {
        StopAttacking();
        attackCoroutine = StartCoroutine(AttackLoop());
    }

    public void StopAttacking()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            if (player != null && player.Hp > 0)
            {
                // Enemy 프리팹에 AttackCollider가 있으면 Attack()으로 애니메이션+충돌 처리.
                // AttackCollider가 없는 경우 DamageCalculator로 직접 피해 적용.
                if (hasAttackCollider)
                {
                    enemy.Attack();
                }
                else
                {
                    var (damage, _) = DamageCalculator.Calculate(
                        enemy.AttackPower,
                        player.DamageReduction,
                        enemy.CriticalPercentage
                    );
                    player.TakeDamage(damage);
                }
            }

            float interval = enemy.AttackSpeed > 0f ? 1f / enemy.AttackSpeed : 2f;
            yield return YieldInstructionCache.WaitForSeconds(interval);
        }
    }
}
