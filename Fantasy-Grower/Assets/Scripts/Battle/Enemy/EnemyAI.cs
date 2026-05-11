using System.Collections;
using UnityEngine;

/// <summary>
/// 적의 자동 공격 AI.
/// BattleManager가 스폰 후 Initialize()와 StartAttacking()을 호출하여 동작을 시작한다.
/// </summary>
[RequireComponent(typeof(Enemy))]
public class EnemyAI : MonoBehaviour, IAttackEvent
{
    private Enemy enemy;
    private Coroutine attackCoroutine;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        enemy.OnDied += OnEnemyDied;
    }

    private void OnDestroy()
    {
        if (enemy != null)
            enemy.OnDied -= OnEnemyDied;
    }

    private void OnEnemyDied(Entity _) => StopAttacking();

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
            enemy.Attack();

            float interval = enemy.AttackSpeed > 0f ? 1f / enemy.AttackSpeed : 2f;
            yield return YieldInstructionCache.WaitForSeconds(interval);
        }
    }
}
