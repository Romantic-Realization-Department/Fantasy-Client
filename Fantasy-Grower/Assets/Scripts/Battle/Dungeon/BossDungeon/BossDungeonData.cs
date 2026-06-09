using UnityEngine;

[CreateAssetMenu(fileName = "BossDungeonData", menuName = "Battle/DungeonData/BossDungeonData")]
public class BossDungeonData : DungeonData
{
    [SerializeField]
    private WaveData[] _waves;

    [SerializeField]
    private float _nextWaveDelay = 1.5f;

    [Header("A Grade Weapon Reward")]
    [SerializeField]
    private WeaponID[] _aGradeWeaponCandidates = { WeaponID.A1, WeaponID.A2 };

    [SerializeField, Min(1)]
    private uint _weaponRewardAmount = 1;

    public WaveData[] Waves => _waves;
    public float NextWaveDelay => _nextWaveDelay;
    public WeaponID[] AGradeWeaponCandidates => _aGradeWeaponCandidates;
    public uint WeaponRewardAmount => _weaponRewardAmount;
}
