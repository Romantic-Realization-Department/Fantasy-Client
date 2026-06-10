using UnityEngine;

[System.Serializable]
public struct WeaponDropEntry
{
    public WeaponID WeaponID;

    [Range(0f, 100f)]
    public float DropChance;

    [Min(1)]
    public uint Amount;
}

[CreateAssetMenu(fileName = "WeaponDungeonData", menuName = "Battle/DungeonData/WeaponDungeonData")]
public class WeaponDungeonData : DungeonData
{
    [SerializeField]
    private WaveData[] _waves;

    [SerializeField]
    private float _nextWaveDelay = 1.5f;

    [Header("Weapon Drops")]
    [SerializeField]
    private WeaponDropEntry[] _weaponDrops;

    [Header("Upgrade Scroll Drops")]
    [SerializeField, Range(0f, 100f)]
    private float _upgradeScrollDropChance;

    [SerializeField, Min(1)]
    private uint _upgradeScrollDropAmount = 1;

    public WaveData[] Waves => _waves;
    public float NextWaveDelay => _nextWaveDelay;
    public WeaponDropEntry[] WeaponDrops => _weaponDrops;
    public float UpgradeScrollDropChance => _upgradeScrollDropChance;
    public uint UpgradeScrollDropAmount => _upgradeScrollDropAmount;
}
