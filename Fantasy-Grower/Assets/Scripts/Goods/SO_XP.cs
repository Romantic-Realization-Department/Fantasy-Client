using System;
using UnityEngine;

[CreateAssetMenu(fileName = "XP", menuName = "ScriptableObjects/Goods/XP", order = 2)]
public class SO_XP : SO_Goods
{
    [Obsolete]
    public override void Decrease(uint amount)
    {
        // 감소 기능 삭제 (경험치는 감소하지 않음)
    }
}
