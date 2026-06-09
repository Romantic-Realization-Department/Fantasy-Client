using UnityEngine;

[CreateAssetMenu(fileName = "EntityStat", menuName = "Stat/Entity")]
public class EntityStatData : ScriptableObject
{
    public int Hp;
    public float HpRecovery;
    public float DamageReduction;
    public int AttackPower;
    public float AttackSpeed;
    public float CriticalPercentage;
}
