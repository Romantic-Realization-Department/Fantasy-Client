using UnityEngine;

[CreateAssetMenu(fileName = "BasicDungeonData", menuName = "Battle/DungeonData/BasicDungeonData")]
public class BasicDungeonData : DungeonData
{
    [SerializeField]
    private WaveData[] _waves;

    [SerializeField]
    private float _nextWaveDelay = 1.5f;

    public WaveData[] Waves => _waves;
    public float NextWaveDelay => _nextWaveDelay;
}
