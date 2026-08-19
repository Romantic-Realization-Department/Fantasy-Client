using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("UI/Effects/Gradient")]
public class UIGradient : BaseMeshEffect
{
    [SerializeField, Tooltip("The starting color of the gradient.")]
    private Color startColor = Color.black;

    [SerializeField, Tooltip("The ending color of the gradient.")]
    private Color endColor = Color.white;

    public enum GradientDirection
    {
        Vertical,
        Horizontal,
        DiagonalUp,
        DiagonalDown,
    }

    [SerializeField, Tooltip("The direction of the gradient.")]
    private GradientDirection gradientDirection = GradientDirection.Vertical;

    [
        SerializeField,
        Tooltip(
            "선택 사항: 그라데이션의 고정된 크기 기준이 될 부모 RectTransform (Slider의 경우 Background나 Slider 본체 할당)"
        )
    ]
    private RectTransform referenceRect;

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount <= 0 || graphic == null)
            return;

        UIVertex vertex = new();

        // 기준이 될 RectTransform 설정 (할당되지 않았다면 자기 자신)
        RectTransform targetRectTransform =
            referenceRect != null ? referenceRect : graphic.rectTransform;
        Rect rect = targetRectTransform.rect;
        float minX = rect.xMin;
        float minY = rect.yMin;
        float width = rect.width;
        float height = rect.height;

        // 좌표계 변환 행렬 (자신의 로컬 좌표 -> referenceRect의 로컬 좌표)
        bool useReferenceRect = referenceRect != null && referenceRect != graphic.rectTransform;
        Matrix4x4 localToRefLocal = useReferenceRect
            ? referenceRect.worldToLocalMatrix * graphic.rectTransform.localToWorldMatrix
            : Matrix4x4.identity;

        // 루프 내 캐스팅 오버헤드 방지
        Color32 startColor32 = startColor;
        Color32 endColor32 = endColor;

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            // 버텍스 좌표를 기준 RectTransform의 로컬 좌표계로 변환
            Vector3 pos = useReferenceRect
                ? localToRefLocal.MultiplyPoint3x4(vertex.position)
                : vertex.position;

            // 변환된 좌표가 전체 기준 영역 중 어느 비율에 해당하는지 계산 (0 ~ 1)
            float normalizedX = width > 0 ? (pos.x - minX) / width : 0f;
            float normalizedY = height > 0 ? (pos.y - minY) / height : 0f;

            float normalizedValue = gradientDirection switch
            {
                GradientDirection.Vertical => normalizedY,
                GradientDirection.Horizontal => normalizedX,
                GradientDirection.DiagonalUp => (normalizedX + normalizedY) / 2f,
                GradientDirection.DiagonalDown => (normalizedX + (1 - normalizedY)) / 2f,
                _ => normalizedY,
            };

            // 보간된 색상 계산
            Color32 gradColor = Color32.Lerp(startColor32, endColor32, normalizedValue);

            // 기존 그래픽(Image 등)이 가지는 고유 색상 및 투명도(Alpha)를 보존하기 위해 곱연산 적용
            vertex.color = new Color32(
                (byte)(vertex.color.r * gradColor.r / 255),
                (byte)(vertex.color.g * gradColor.g / 255),
                (byte)(vertex.color.b * gradColor.b / 255),
                (byte)(vertex.color.a * gradColor.a / 255)
            );

            vh.SetUIVertex(vertex, i);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        // 컴포넌트가 붙어있는 그래픽(Image, Text 등)의 메쉬를 갱신
        if (graphic != null)
        {
            graphic.SetVerticesDirty();
        }
    }
#endif
}
