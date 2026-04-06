using UnityEngine;

/// <summary>
/// 적 사망 시 지급되는 보상 데이터.
/// Enemy 프리팹에 할당되며, 같은 적 종류는 하나의 SO 에셋을 공유한다.
/// </summary>
[CreateAssetMenu(fileName = "EnemyRewardData", menuName = "Battle/EnemyRewardData")]
public class EnemyRewardData : ScriptableObject
{
    [Header("사망 보상")]
    public uint goldAmount;
    public uint xpAmount;
}
