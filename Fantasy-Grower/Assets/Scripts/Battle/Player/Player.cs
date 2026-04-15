using System.Collections;
using UnityEngine;

public class Player : Entity
{
    private AttackCollider col;

    protected override void Awake()
    {
        base.Awake();
        col = GetComponentInChildren<AttackCollider>();
        col.gameObject.SetActive(false);
    }

    private bool isAttacking;

    public override void Attack()
    {
        if (isAttacking)
            return;
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        col.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f); // 히트 판정 윈도우 (고정)
        col.gameObject.SetActive(false);
        isAttacking = false;
    }
}
