using UnityEngine;

/// <summary>
/// 두 SpriteRenderer를 사용해 적의 현재 체력을 표시하고 지정된 월드 오프셋 위치를 따라갑니다.
/// Fill Sprite의 피벗 위치와 관계없이 왼쪽 경계를 유지하며 오른쪽부터 게이지가 줄어듭니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class EnemyHealthBar : MonoBehaviour
{
    [Header("대상")]
    [SerializeField]
    private Enemy targetEnemy;

    [SerializeField]
    private Vector3 worldOffset = new(0f, 1.5f, 0f);

    [Header("체력바 렌더러")]
    [SerializeField]
    private SpriteRenderer backgroundRenderer;

    [SerializeField]
    private SpriteRenderer fillRenderer;

    private Vector3 initialFillLocalPosition;
    private Vector3 initialFillLocalScale;
    private float fillBoundsMinX;
    private bool hasCachedFillGeometry;
    private float previousRatio = -1f;

    private void Awake()
    {
        if (targetEnemy == null)
            targetEnemy = GetComponentInParent<Enemy>();

        CacheFillGeometry();
    }

    private void OnEnable()
    {
        previousRatio = -1f;
        CacheFillGeometry();
        RefreshHealth();
    }

    private void LateUpdate()
    {
        if (targetEnemy == null)
        {
            SetRenderersVisible(false);
            enabled = false;
            return;
        }

        transform.position = targetEnemy.transform.position + worldOffset;
        RefreshHealth();
    }

    private void RefreshHealth()
    {
        // 예외 처리
        if (targetEnemy == null || fillRenderer == null || fillRenderer.sprite == null)
            return;

        // 현재 체력 비율 계산
        float ratio = targetEnemy.MaxHp > 0f ? targetEnemy.Hp / targetEnemy.MaxHp : 0f;
        ratio = Mathf.Clamp01(ratio);

        // 체력 비율이 이전과 동일하면 갱신하지 않음
        if (Mathf.Approximately(previousRatio, ratio))
            return;

        // 체력 비율이 변경되었으므로 갱신
        previousRatio = ratio;

        // Fill Sprite의 스케일을 체력 비율에 맞게 조정
        Vector3 fillScale = initialFillLocalScale;
        fillScale.x *= ratio;
        fillRenderer.transform.localScale = fillScale;

        // Sprite의 최초 왼쪽 경계를 보존하도록 스케일 변화량만큼 위치를 보정합니다.
        Vector3 fillPosition = initialFillLocalPosition;
        fillPosition.x += fillBoundsMinX * (initialFillLocalScale.x - fillScale.x);
        fillRenderer.transform.localPosition = fillPosition;
    }

    private void CacheFillGeometry()
    {
        if (hasCachedFillGeometry || fillRenderer == null || fillRenderer.sprite == null)
            return;

        initialFillLocalPosition = fillRenderer.transform.localPosition;
        initialFillLocalScale = fillRenderer.transform.localScale;
        fillBoundsMinX = fillRenderer.sprite.bounds.min.x;
        hasCachedFillGeometry = true;
    }

    private void SetRenderersVisible(bool isVisible)
    {
        if (backgroundRenderer != null)
            backgroundRenderer.enabled = isVisible;

        if (fillRenderer != null)
            fillRenderer.enabled = isVisible;
    }

    private void OnValidate()
    {
        if (targetEnemy == null)
            targetEnemy = GetComponentInParent<Enemy>();

        if (targetEnemy == null)
        {
            Debug.LogError(
                "[EnemyHealthBar] 부모에서 Enemy를 찾을 수 없습니다. Target Enemy를 할당해주세요.",
                this
            );
        }

        if (backgroundRenderer == null)
        {
            Debug.LogError("[EnemyHealthBar] Background Renderer가 할당되지 않았습니다.", this);
        }

        if (fillRenderer == null)
        {
            Debug.LogError("[EnemyHealthBar] Fill Renderer가 할당되지 않았습니다.", this);
        }
        else if (fillRenderer.sprite == null)
        {
            Debug.LogError("[EnemyHealthBar] Fill Renderer에 Sprite가 없습니다.", this);
        }
    }
}
