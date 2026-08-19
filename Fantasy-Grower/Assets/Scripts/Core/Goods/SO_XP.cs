using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EachLevelNeedXp : IEnumerable<uint>
{
    private readonly uint[] eachLevelNeedXP = new uint[SO_Level.MAX_LEVEL];

    public uint this[int index] => eachLevelNeedXP[index];

    public EachLevelNeedXp()
    {
        uint needXP = 100;
        for (int i = 0; i < SO_Level.MAX_LEVEL; i++)
        {
            eachLevelNeedXP[i] = needXP;
            needXP *= 2;
        }
    }

    public IEnumerator<uint> GetEnumerator()
    {
        return ((IEnumerable<uint>)eachLevelNeedXP).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

[CreateAssetMenu(fileName = "XP", menuName = "ScriptableObjects/Goods/XP", order = 2)]
public class SO_XP : SO_Goods
{
    protected override string GoodsName { get; } = "경험치";

    /// <summary>
    /// 레벨별 필요 경험치 명시
    /// </summary>
    public static readonly EachLevelNeedXp NeedXpTable = new();

    public override bool Decrease(uint amount)
    {
        // 감소 기능 삭제 (경험치는 감소하지 않음)
        Debug.LogWarning("경험치는 감소시킬 수 없습니다!!!");
        return false;
    }
}
