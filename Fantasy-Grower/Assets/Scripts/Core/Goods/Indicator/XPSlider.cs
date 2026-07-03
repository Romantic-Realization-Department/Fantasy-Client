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
    private SO_Goods xp;

    private void Awake()
    {
        xpBar = GetComponent<Slider>();
    }

    private void Start()
    {
        xp = GoodsManager.Instance.GetGoods(type);
        if (xp == null)
            return;

        GetCurrentXPRatio(xp.Get());
        xp.OnValueChange += GetCurrentXPRatio;
    }

    private void OnDestroy()
    {
        if (xp != null)
            xp.OnValueChange -= GetCurrentXPRatio;
    }

    private void GetCurrentXPRatio(uint currentXP)
    {
        for (int level = 1; level < SO_Level.MAX_LEVEL; level++)
        {
            uint nextLevelExp = SO_XP.NeedXpTable[level - 1];
            if (currentXP < nextLevelExp)
            {
                // XPRatioText가 Slider.onValueChanged를 통해 갱신되므로 이벤트를 함께 발생시킵니다.
                xpBar.value = (float)currentXP / nextLevelExp;
                return;
            }
            else
            {
                currentXP -= nextLevelExp;
            }
        }

        xpBar.value = 1f;
    }
}
