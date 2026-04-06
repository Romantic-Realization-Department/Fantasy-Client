using System.Collections;
using UnityEngine;

public class TestEnemy : Enemy
{
    private AttackCollider col;

    protected override void Awake()
    {
        base.Awake();
        col = GetComponentInChildren<AttackCollider>();
        col.gameObject.SetActive(false);
    }

    public override void Death()
    {
        Debug.Log("TestEnemy 죽음");
        base.Death(); // Enemy.Death() → 보상 지급 + OnDied 이벤트
    }

    private bool _isAttacking;

    public override void Attack()
    {
        if (_isAttacking)
            return;
        StartCoroutine(attackCoroutine());
    }

    private IEnumerator attackCoroutine()
    {
        _isAttacking = true;
        col.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f); // 히트 판정 윈도우 (고정)
        col.gameObject.SetActive(false);
        _isAttacking = false;
    }
}
