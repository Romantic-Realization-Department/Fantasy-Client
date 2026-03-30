using UnityEngine;

[CreateAssetMenu(fileName = "EntityStat", menuName = "Stat/Entity")]
public class EntityStatData : ScriptableObject
{
    public int Hp;
    public int AttackPower;
    public float CriticalPercentage;
}
