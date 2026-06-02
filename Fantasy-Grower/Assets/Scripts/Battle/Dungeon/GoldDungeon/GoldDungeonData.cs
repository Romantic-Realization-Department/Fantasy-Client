using UnityEngine;

[CreateAssetMenu(fileName = "GoldDungeonData", menuName = "Battle/DungeonData/GoldDungeonData")]
public class GoldDungeonData : DungeonData
{
    [SerializeField, Tooltip("피해량 비례 획득하는 골드의 양")]
    private float _goldPerDamage = 1f;

    [SerializeField, Tooltip("미스릴이 드롭될 확률")]
    private float _mithrilDropPercent;

    [SerializeField, Tooltip("미스릴이 한 번에 드롭될 개수")]
    private uint _mithrilDropAmount;

    public float GoldPerDamage => _goldPerDamage;
    public float MithrilDropChance => _mithrilDropPercent;
    public uint MithrilDropAmount => _mithrilDropAmount;
}
