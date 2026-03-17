using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeScroll", menuName = "ScriptableObjects/Goods/UpgradeScroll", order = 4)]
public class SO_UpgradeScroll : SO_Goods
{
    public override void Decrease(uint amount)
    {
        if (amount > value)
        {
            // 강화 스크롤이 부족한 경우
            Debug.LogError($"강화 스크롤이 {amount - value}만큼 부족합니다!!!");
            return;
        }
    
        base.Decrease(amount);
    }
}
