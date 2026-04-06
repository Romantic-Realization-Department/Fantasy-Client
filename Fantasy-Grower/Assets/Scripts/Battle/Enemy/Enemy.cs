using UnityEngine;

/// <summary>
/// 적 엔티티 기반 클래스.
/// 사망 시 EnemyRewardData에 정의된 Gold/XP를 지급한다.
/// </summary>
public class Enemy : Entity
{
    [SerializeField]
    private EnemyRewardData _rewardData;

    [SerializeField]
    private SO_Gold _gold;

    [SerializeField]
    private SO_XP _xp;

    public override void Death()
    {
        base.Death(); // OnDied 이벤트 발화 (WaveController가 구독 중)

        if (_rewardData != null)
        {
            _gold?.Increase(_rewardData.goldAmount);
            _xp?.Increase(_rewardData.xpAmount);
        }

        Destroy(gameObject, 0.5f); // 사망 연출 시간 확보 후 제거
    }
}
