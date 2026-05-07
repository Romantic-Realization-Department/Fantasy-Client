using UnityEngine;

namespace DG.Tweening
{
    public static class DOTweenExtension
    {
        public static Tweener DOColor(
            this SpriteRenderer[] spriteRenderers,
            Color endValue,
            float duration
        )
        {
            Color[] baseColor = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                baseColor[i] = spriteRenderers[i].color;
            }

            return DOVirtual.Float(
                0,
                1,
                duration,
                (t) =>
                {
                    for (int i = 0; i < baseColor.Length; i++)
                    {
                        spriteRenderers[i].color = Color.Lerp(baseColor[i], endValue, t);
                    }
                }
            );
        }
    }
}
