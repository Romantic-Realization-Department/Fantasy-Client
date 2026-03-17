using UnityEngine;

[CreateAssetMenu(fileName = "Mithril", menuName = "ScriptableObjects/Goods/Mithril", order = 5)]
public class SO_Mithril : SO_Goods
{
    public override void Decrease(uint amount)
    {
        if (amount > value)
        {
            // 미스릴이 부족한 경우
            Debug.LogError($"미스릴이 {amount - value}만큼 부족합니다!!!");
            return;
        }
    
        base.Decrease(amount);
    }
}
