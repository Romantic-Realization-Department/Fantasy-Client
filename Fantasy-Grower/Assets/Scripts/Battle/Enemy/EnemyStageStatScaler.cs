using UnityEngine;

/// <summary>
/// 현재 스테이지에 맞춰 적의 기본 능력치에 성장 보정값을 적용합니다.
/// 적이 생성될 때 한 번 계산하며, 스테이지가 끝날 때까지 같은 보정값을 유지합니다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Enemy))]
public sealed class EnemyStageStatScaler : MonoBehaviour
{
    [Header("성장 기준")]
    [SerializeField, Min(1)]
    [Tooltip("보정 없이 기본 능력치를 사용하는 1-based 기준 스테이지입니다.")]
    private int baseStage = 1;

    [Header("스테이지당 성장량")]
    [SerializeField]
    [Tooltip("고정값은 스테이지마다 선형으로 더해집니다. 비율은 0.1을 입력하면 10%를 의미합니다.")]
    private EntityStatModifier growthPerStage = new()
    {
        BonusHpRate = 0.3f,
        BonusHpRecoveryRate = 0.3f,
        BonusAttackPowerRate = 0.22f,
    };

    [SerializeField]
    [Tooltip(
        "활성화하면 비율 성장량을 복리로 계산합니다. 예: 매 스테이지 10%이면 2회 성장 후 21%입니다."
    )]
    private bool useCompoundRate = true;

    private static IStageProvider cachedStageProvider;

    private Enemy enemy;
    private EntityStatModifierHandle stageModifierHandle;

    public int CurrentStageNumber { get; private set; } = 1;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        ApplyCurrentStageModifier();
    }

    private void OnDisable()
    {
        if (enemy != null && stageModifierHandle.IsValid)
            enemy.RemoveStatModifier(stageModifierHandle);

        stageModifierHandle = default;
    }

    /// <summary>
    /// 현재 씬의 스테이지 제공자를 조회해 적에게 성장 보정값을 적용합니다.
    /// </summary>
    public void ApplyCurrentStageModifier()
    {
        IStageProvider stageProvider = FindStageProvider();
        if (stageProvider == null)
        {
            Debug.LogWarning(
                "[EnemyStageStatScaler] IStageProvider를 찾지 못해 기본 능력치를 사용합니다.",
                this
            );
            return;
        }

        ApplyStageModifier(stageProvider.CurrentStageIndex);
    }

    /// <summary>
    /// 0-based 스테이지 인덱스를 기준으로 성장 보정값을 계산해 적용합니다.
    /// 테스트나 외부 스폰 시스템에서 스테이지를 직접 지정할 때 사용할 수 있습니다.
    /// </summary>
    public void ApplyStageModifier(int stageIndex)
    {
        if (enemy == null)
            return;

        int stageNumber = Mathf.Max(1, stageIndex + 1);
        CurrentStageNumber = stageNumber;
        int growthCount = Mathf.Max(0, stageNumber - baseStage);
        EntityStatModifier modifier = EntityStatModifierCalculator.ScalePerStep(
            growthPerStage,
            growthCount,
            useCompoundRate
        );

        if (stageModifierHandle.IsValid)
        {
            if (!enemy.UpdateStatModifier(stageModifierHandle, modifier))
                stageModifierHandle = enemy.ApplyStatModifier(modifier);
        }
        else
        {
            stageModifierHandle = enemy.ApplyStatModifier(modifier);
        }
    }

    private static IStageProvider FindStageProvider()
    {
        if (cachedStageProvider is MonoBehaviour cachedBehaviour && cachedBehaviour != null)
        {
            return cachedStageProvider;
        }

        cachedStageProvider = null;
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is not IStageProvider stageProvider)
                continue;

            cachedStageProvider = stageProvider;
            return cachedStageProvider;
        }

        return null;
    }

    private void OnValidate()
    {
        baseStage = Mathf.Max(1, baseStage);

        EntityStatModifier validatedGrowth = growthPerStage;
        validatedGrowth.BonusHpRate = Mathf.Max(0f, validatedGrowth.BonusHpRate);
        validatedGrowth.BonusHpRecoveryRate = Mathf.Max(0f, validatedGrowth.BonusHpRecoveryRate);
        validatedGrowth.BonusDamageReductionRate = Mathf.Max(
            0f,
            validatedGrowth.BonusDamageReductionRate
        );
        validatedGrowth.BonusAttackPowerRate = Mathf.Max(0f, validatedGrowth.BonusAttackPowerRate);
        validatedGrowth.BonusAttackSpeedRate = Mathf.Max(0f, validatedGrowth.BonusAttackSpeedRate);
        validatedGrowth.BonusCriticalPercentageRate = Mathf.Max(
            0f,
            validatedGrowth.BonusCriticalPercentageRate
        );
        growthPerStage = validatedGrowth;
    }
}
