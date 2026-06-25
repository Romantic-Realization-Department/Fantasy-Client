using UnityEngine;

/// <summary>
/// 적 엔티티 기반 클래스.
/// 사망 시 EnemyRewardData에 정의된 Gold/XP를 지급한다.
/// </summary>
public class Enemy : Entity
{
    [SerializeField, Header("공격 설정")]
    protected AttackTargetsSensing targets;

    [Header("처치 보상 설정")]
    [SerializeField]
    private EnemyRewardData rewardData;

    [Header("움직임 설정")]
    [SerializeField]
    protected Rigidbody2D rb;

    [SerializeField]
    protected float moveSpeed = 3f;

    [SerializeField]
    protected ForwardColleagueSensor sensor;

    protected override void Awake()
    {
        base.Awake();
        sensor.OnBlocked += OnBlocked;
        sensor.OnUnBlocked += OnUnBlocked;
    }

    protected override void Start()
    {
        entityState[gameObject].State = PlayerState.MOVE;
    }

    protected override void OnMove()
    {
        rb.linearVelocityX = -moveSpeed;
    }

    protected override void OnIdle()
    {
        rb.linearVelocityX = 0f;
    }

    protected override void OnAttack()
    {
        OnIdle(); // 공격 중에는 움직이지 않도록 설정
    }

    protected override void OnDeath()
    {
        rb.linearVelocityX = 0f;
    }

    // 앞에 아군이 감지 되었을 때
    private void OnBlocked()
    {
        entityState[gameObject].State = PlayerState.IDLE;
    }

    // 앞에 아군이 사라졌을 때
    private void OnUnBlocked()
    {
        entityState[gameObject].State = PlayerState.MOVE;
    }

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

    protected override void OnDestroy()
    {
        base.OnDestroy();

        sensor.OnBlocked -= OnBlocked;
        sensor.OnUnBlocked -= OnUnBlocked;
    }

    protected override void OnValidate()
    {
        if (!targets)
            Debug.LogError(
                "[Entity] Targets 필드에 AttackTargetsSensing 컴포넌트를 할당해주세요.",
                this
            );

        base.OnValidate();

        if (!rewardData)
            Debug.LogWarning(
                "[Enemy] RewardData가 할당되지 않았습니다. 처치 보상이 지급되지 않습니다.",
                this
            );

        if (!rb)
            Debug.LogWarning(
                "[Enemy] Rigidbody2D가 할당되지 않았습니다. 움직임이 정상적으로 작동하지 않을 수 있습니다.",
                this
            );
    }
}
