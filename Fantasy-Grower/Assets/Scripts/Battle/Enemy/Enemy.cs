using UnityEngine;

public enum EnemyGrade
{
    Normal,
    Elite,
    Boss,
}

/// <summary>
/// 적 엔티티 기반 클래스.
/// 사망 시 EnemyRewardData에 정의된 Gold/XP를 지급한다.
/// </summary>
public class Enemy : Entity
{
    [field: SerializeField, Header("적 등급")]
    public EnemyGrade Grade { get; private set; }

    public bool IsEliteTarget => Grade is EnemyGrade.Elite or EnemyGrade.Boss;

    [SerializeField, Header("공격 방식")]
    private EntityAttackBehaviour attackBehaviour;

    [Header("처치 보상 설정")]
    [SerializeField]
    private EnemyRewardData rewardData;

    private EnemyStageStatScaler stageStatScaler;

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
        ResolveAttackBehaviour();
        stageStatScaler = GetComponent<EnemyStageStatScaler>();
        sensor.OnBlocked += OnBlocked;
        sensor.OnUnBlocked += OnUnBlocked;
        OnStatsChanged += HandleStatsChanged;
    }

    protected override void Start()
    {
        entityState[gameObject].State = PlayerState.MOVE;
    }

    protected override void OnMove()
    {
        rb.linearVelocityX = -moveSpeed * MoveSpeedMultiplier;
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

    public override void Attack()
    {
        // 프로젝트 규칙에 따라 피해 판정을 먼저 완료한 뒤 공격 애니메이션을 실행합니다.
        if (attackBehaviour == null || !attackBehaviour.TryAttack(this))
            return;

        base.Attack();
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

    private void UnsubscribeSensorEvents()
    {
        sensor.OnBlocked -= OnBlocked;
        sensor.OnUnBlocked -= OnUnBlocked;
    }

    private void HandleStatsChanged()
    {
        if (entityState[gameObject].State == PlayerState.MOVE)
            OnMove();
    }

    public override void Death()
    {
        base.Death(); // OnDied 이벤트 발화 (WaveController가 구독 중)

        UnsubscribeSensorEvents();

        if (rewardData != null)
        {
            int stageNumber = stageStatScaler != null ? stageStatScaler.CurrentStageNumber : 1;
            uint goldReward = rewardData.CalculateGold(stageNumber);
            uint xpReward = rewardData.CalculateXp(stageNumber);

            GoodsManager.Instance.GetGoods(GoodsType.Gold).Increase(goldReward);
            GoodsManager.Instance.GetGoods(GoodsType.XP).Increase(xpReward);
        }

        Destroy(gameObject, 0.5f); // 사망 연출 시간 확보 후 제거
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        OnStatsChanged -= HandleStatsChanged;

        // 예외 처리(만약 적이 죽어서 파괴된 게 아니라면)
        UnsubscribeSensorEvents();
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        ResolveAttackBehaviour();

        if (sensor == null)
        {
            sensor = GetComponentInChildren<ForwardColleagueSensor>();
            if (sensor == null)
                Debug.LogError("[Enemy] ForwardColleagueSensor를 할당해주세요.", this);
        }

        if (attackBehaviour == null)
            Debug.LogError("[Enemy] EntityAttackBehaviour를 할당해주세요.", this);

        if (!rewardData)
            Debug.LogWarning(
                "[Enemy] RewardData가 할당되지 않았습니다. 처치 보상이 지급되지 않습니다.",
                this
            );

        if (GetComponent<EnemyStageStatScaler>() == null)
        {
            Debug.LogWarning(
                "[Enemy] EnemyStageStatScaler가 없어 1스테이지 기본 보상을 지급합니다.",
                this
            );
        }

        if (!rb)
            Debug.LogWarning(
                "[Enemy] Rigidbody2D가 할당되지 않았습니다. 움직임이 정상적으로 작동하지 않을 수 있습니다.",
                this
            );
    }

    private void ResolveAttackBehaviour()
    {
        if (attackBehaviour == null)
            attackBehaviour = GetComponent<EntityAttackBehaviour>();
    }
}
