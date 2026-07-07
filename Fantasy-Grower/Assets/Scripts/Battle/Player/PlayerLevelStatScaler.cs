using UnityEngine;

/// <summary>
/// 현재 플레이어 레벨에 해당하는 기본 성장분을 Entity 스탯 보정 시스템에 적용합니다.
/// 직업별 Player 프리팹마다 다른 성장값을 설정할 수 있으며 장비, 패시브, 버프와 독립적으로 중첩됩니다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
public sealed class PlayerLevelStatScaler : MonoBehaviour
{
    [Header("레벨 성장 기준")]
    [SerializeField, Min(1)]
    [Tooltip("성장 보정 없이 직업 고유의 EntityStatData를 그대로 사용하는 기준 레벨입니다.")]
    private int baseLevel = 1;

    [Header("레벨당 성장량")]
    [SerializeField]
    [Tooltip("고정값은 레벨마다 더해지고, 비율은 0.08 입력 시 레벨마다 8% 성장합니다.")]
    private EntityStatModifier growthPerLevel = new()
    {
        BonusHpRate = 0.21f,
        BonusHpRecoveryRate = 0.27f,
        BonusAttackPowerRate = 0.31f,
    };

    [SerializeField]
    [Tooltip("활성화하면 레벨별 비율 성장값을 복리로 계산합니다.")]
    private bool useCompoundRate = true;

    private Player player;
    private SO_Level level;
    private EntityStatModifierHandle levelModifierHandle;
    private bool hasStarted;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Start()
    {
        hasStarted = true;
        BindLevel();
    }

    private void OnEnable()
    {
        if (hasStarted)
            BindLevel();
    }

    private void OnDisable()
    {
        UnbindLevel();

        if (player != null && levelModifierHandle.IsValid)
            player.RemoveStatModifier(levelModifierHandle);

        levelModifierHandle = default;
    }

    private void BindLevel()
    {
        if (level != null)
            return;

        GoodsManager goodsManager = GoodsManager.Instance;
        if (goodsManager == null)
            return;

        level = goodsManager.GetGoods(GoodsType.Level) as SO_Level;
        if (level == null)
        {
            Debug.LogError("[PlayerLevelStatScaler] GoodsManager에 SO_Level을 등록해주세요.", this);
            return;
        }

        level.OnValueChange += ApplyLevelModifier;
        ApplyLevelModifier(level.Get());
    }

    private void UnbindLevel()
    {
        if (level == null)
            return;

        level.OnValueChange -= ApplyLevelModifier;
        level = null;
    }

    private void ApplyLevelModifier(uint currentLevel)
    {
        int validatedLevel = Mathf.Max(1, (int)currentLevel);
        int growthCount = Mathf.Max(0, validatedLevel - baseLevel);
        EntityStatModifier modifier = EntityStatModifierCalculator.ScalePerStep(
            growthPerLevel,
            growthCount,
            useCompoundRate
        );

        if (levelModifierHandle.IsValid)
        {
            if (!player.UpdateStatModifier(levelModifierHandle, modifier))
                levelModifierHandle = player.ApplyStatModifier(modifier);
        }
        else
        {
            levelModifierHandle = player.ApplyStatModifier(modifier);
        }
    }

    private void OnValidate()
    {
        baseLevel = Mathf.Max(1, baseLevel);

        EntityStatModifier validatedGrowth = growthPerLevel;
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
        growthPerLevel = validatedGrowth;
    }
}
