using UnityEngine;

public static class UIHelper
{
    public static string GetSpriteTag(GoodsType type)
    {
        // 컴파일 타입만을 사용하여 GC할당 Zero 유지
        return type switch
        {
            GoodsType.Gold => "<sprite name=\"" + nameof(GoodsType.Gold) + "_Icon\"> : ",
            GoodsType.Mithril => "<sprite name=\"" + nameof(GoodsType.Mithril) + "_Icon\"> : ",
            GoodsType.XP => "<sprite name=\"" + nameof(GoodsType.XP) + "_Icon\"> : ",
            _ => "",
        };
    }
}
