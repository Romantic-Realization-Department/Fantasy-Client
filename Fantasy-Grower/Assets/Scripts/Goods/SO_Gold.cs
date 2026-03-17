using UnityEngine;

[CreateAssetMenu(fileName = "Gold", menuName = "ScriptableObjects/Goods/Gold", order = 1)]
public class SO_Gold : SO_Goods
{
    public override void Decrease(uint amount)
    {
        if (amount > value)
        {
            // 골드가 부족한 경우
            Debug.LogError($"골드가 {amount - value}만큼 부족합니다!!!");
            return;
        }

        base.Decrease(amount);
    }
}
