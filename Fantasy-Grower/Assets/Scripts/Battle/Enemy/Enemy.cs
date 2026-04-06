using UnityEngine;

/// <summary>
/// 적 엔티티 기반 클래스.
/// 사망 시 EnemyRewardData에 정의된 Gold/XP를 지급한다.
/// </summary>
public class Enemy : Entity
{
    [SerializeField]
    private EnemyRewardData rewardData;

    [SerializeField]
    private SO_Gold gold;

    [SerializeField]
    private SO_XP xp;

    public override void Death()
    {
        base.Death(); // OnDied 이벤트 발화 (WaveController가 구독 중)

        if (rewardData != null)
        {
            gold?.Increase(rewardData.GoldAmount);
            xp?.Increase(rewardData.XpAmount);
        }

        Destroy(gameObject, 0.5f); // 사망 연출 시간 확보 후 제거
    }
}
