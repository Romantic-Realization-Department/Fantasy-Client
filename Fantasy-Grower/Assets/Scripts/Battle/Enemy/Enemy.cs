using UnityEngine;

/// <summary>
/// 적 엔티티 기반 클래스.
/// 사망 시 EnemyRewardData에 정의된 Gold/XP를 지급한다.
/// </summary>
public class Enemy : Entity
{
    [SerializeField]
    private EnemyRewardData rewardData;

    public override void Death()
    {
        base.Death(); // OnDied 이벤트 발화 (WaveController가 구독 중)

        if (rewardData != null)
        {
            GoodsManager.Instance.GetGoods(GoodsType.Gold).Increase(rewardData.GoldAmount);
            GoodsManager.Instance.GetGoods(GoodsType.XP).Increase(rewardData.XpAmount);
        }

        Destroy(gameObject, 0.5f); // 사망 연출 시간 확보 후 제거
    }
}
