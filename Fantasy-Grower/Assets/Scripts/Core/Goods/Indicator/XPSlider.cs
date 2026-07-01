using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 다음 레벨까지의 상대적인 경험치 게이지를 표시하는 UI 컴포넌트입니다.
/// </summary>
[RequireComponent(typeof(Slider))]
public class XPSlider : MonoBehaviour
{
    private const GoodsType type = GoodsType.XP;

    private Slider xpBar;

    private void Awake()
    {
        xpBar = GetComponent<Slider>();
        xpBar.value = GetCurrentXPRatio();
    }

    private float GetCurrentXPRatio()
    {
        var currentXP = GoodsManager.Instance.GetGoods(type).Get();

        foreach (var nextLevelExp in SO_XP.NeedXpTable)
        {
            if (currentXP < nextLevelExp)
            {
                return (float)currentXP / nextLevelExp;
            }
            else
            {
                currentXP -= nextLevelExp;
            }
        }

        return 0;
    }
}
