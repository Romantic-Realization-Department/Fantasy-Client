using UnityEngine;

[CreateAssetMenu(fileName = "EntityStat", menuName = "Stat/Entity")]
public class EntityStatData : ScriptableObject
{
    public float Hp;
    public float HpRecovery;
    public float DamageReduction;
    public float AttackPower;
    public float AttackSpeed;
    public float CriticalPercentage;
    public float AttackRange;
}
