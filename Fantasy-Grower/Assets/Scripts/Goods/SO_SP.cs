using UnityEngine;

[CreateAssetMenu(fileName = "SP", menuName = "ScriptableObjects/Goods/SP", order = 3)]
public class SO_SP : SO_Goods
{
    public override void Decrease(uint amount)
    {
        if (amount > value)
        {
            // 스킬 포인트가 부족한 경우
            Debug.LogError($"스킬 포인트가 {amount - value}만큼 부족합니다!!!");
            return;
        }

        base.Decrease(amount);
    }
}
