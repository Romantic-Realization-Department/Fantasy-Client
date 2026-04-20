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

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount <= 0)
            return;

        UIVertex vertex = new();

        // UI 요소의 가장 낮은 곳의 좌표와 가장 높은 곳의 좌표를 계산
        var (min, max) = GetBounds(vh);
        float width = max.x - min.x;
        float height = max.y - min.y;

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            // 현재 버텍스의 Y 위치가 전체 높이 중 어느 비율에 해당하는지 계산 (0 ~ 1)
            float normalizedX = width > 0 ? (vertex.position.x - min.x) / width : 0f;
            float normalizedY = height > 0 ? (vertex.position.y - min.y) / height : 0f;

            float normalizedValue = gradientDirection switch
            {
                GradientDirection.Vertical => normalizedY,
                GradientDirection.Horizontal => normalizedX,
                GradientDirection.DiagonalUp => (normalizedX + normalizedY) / 2f,
                GradientDirection.DiagonalDown => (normalizedX + (1 - normalizedY)) / 2f,
                _ => normalizedY,
            };

            // 보간된 색상을 버텍스 컬러에 적용
            vertex.color = Color32.Lerp(startColor, endColor, normalizedValue);
            vh.SetUIVertex(vertex, i);
        }
    }

    private (Vector2, Vector2) GetBounds(VertexHelper vh)
    {
        float minX = float.MaxValue,
            minY = float.MaxValue;
        float maxX = float.MinValue,
            maxY = float.MinValue;
        UIVertex vertex = new();

        for (int i = 0; i < vh.currentVertCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            // Mathf.Min/Max 호출보다 직접 비교가 연산이 더 빠릅니다.
            if (vertex.position.x < minX)
                minX = vertex.position.x;
            if (vertex.position.y < minY)
                minY = vertex.position.y;
            if (vertex.position.x > maxX)
                maxX = vertex.position.x;
            if (vertex.position.y > maxY)
                maxY = vertex.position.y;
        }

        return (new Vector2(minX, minY), new Vector2(maxX, maxY));
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
