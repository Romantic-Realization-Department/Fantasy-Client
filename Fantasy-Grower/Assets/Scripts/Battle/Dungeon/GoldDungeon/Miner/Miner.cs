using UnityEngine;

/// <summary>
/// 선택한 플레이어의 공격력만을 이어받는 광부입니다. (스킬은 제외)
/// </summary>
public class Miner : Player
{
    [SerializeField]
    private GoldOre _goldOre;

    private SkillTreeComponent skillTreeComponent;

    protected override void Awake()
    {
        ApplySelectedJobBaseStats();
        base.Awake();
        EnsureSkillTreeComponent();
    }

    private void OnEnable()
    {
        GameManager gameManager = GameManager.InstanceOrNull;
        if (gameManager != null)
            gameManager.SetPlayer(this);
    }

    public override void Attack()
    {
        if (_goldOre == null)
            return;

        _goldOre.TakeDamage(CalculateMiningDamage());
        base.Attack();
    }

    private void ApplySelectedJobBaseStats()
    {
        GameManager gameManager = GameManager.InstanceOrNull;
        GameObject selectedPlayerPrefab =
            gameManager != null ? gameManager.GetCurrentPlayerPrefab() : null;

        if (
            selectedPlayerPrefab != null
            && selectedPlayerPrefab.TryGetComponent(out Entity selectedPlayer)
            && selectedPlayer.BaseStatData != null
        )
        {
            statData = selectedPlayer.BaseStatData;
        }
    }

    private void EnsureSkillTreeComponent()
    {
        if (!TryGetComponent(out skillTreeComponent))
            skillTreeComponent = gameObject.AddComponent<SkillTreeComponent>();
    }

    private float CalculateMiningDamage()
    {
        float damage = AttackPower;

        if (skillTreeComponent != null)
        {
            BasicAttackSkillData basicAttack = skillTreeComponent.GetUnlockedBasicAttack();
            if (basicAttack != null)
                damage *= basicAttack.DamageRate;

            damage *= skillTreeComponent.GetOutgoingDamageMultiplier();
            damage *= skillTreeComponent.GetBasicAttackDamageMultiplier();
        }

        damage *= OutgoingDamageMultiplier;
        return damage;
    }
}
