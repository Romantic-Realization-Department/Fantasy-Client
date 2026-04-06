using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어의 자동 공격 타이밍을 관리하는 컴포넌트.
/// AttackSpeed 스탯을 기반으로 초당 공격 횟수를 결정한다.
/// BattleManager가 StartAutoAttack() / StopAutoAttack()으로 제어한다.
/// </summary>
[RequireComponent(typeof(Player))]
public class AutoAttackController : MonoBehaviour
{
    [SerializeField]
    private WaveController _waveController;

    private Player _player;
    private Coroutine _attackCoroutine;

    private void Awake()
    {
        _player = GetComponent<Player>();
    }

    public void StartAutoAttack()
    {
        StopAutoAttack();
        _attackCoroutine = StartCoroutine(AutoAttackLoop());
    }

    public void StopAutoAttack()
    {
        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
            _attackCoroutine = null;
        }
    }

    private IEnumerator AutoAttackLoop()
    {
        while (true)
        {
            if (_waveController.GetFirstAliveEnemy() != null)
                _player.Attack(); // AttackCollider가 실제 피해를 처리

            // AttackSpeed = 초당 공격 횟수 (예: 1.0 → 1초마다 공격)
            float interval = _player.AttackSpeed > 0f ? 1f / _player.AttackSpeed : 1f;
            yield return new WaitForSeconds(interval);
        }
    }
}
