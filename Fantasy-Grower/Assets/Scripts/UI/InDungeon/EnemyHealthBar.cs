using DG.Tweening;
using DG.Tweening.Core;
using UnityEngine;

/// <summary>
/// 적의 자식 오브젝트에서 SpriteRenderer로 현재 체력을 표시합니다.
/// Fill은 체력 변화에 즉시 반응하고, HealthDecreaseBar는 피해량을 보여준 뒤 Tween으로 따라갑니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class EnemyHealthBar : MonoBehaviour
{
    [Header("대상")]
    [SerializeField]
    private Enemy targetEnemy;

    [Header("체력바 렌더러")]
    [SerializeField]
    private SpriteRenderer backgroundRenderer;

    [SerializeField]
    private SpriteRenderer healthDecreaseRenderer;

    [SerializeField]
    private SpriteRenderer fillRenderer;

    [Header("감소 연출")]
    [SerializeField, Min(0f)]
    private float decreaseDelay = 0.15f;

    [SerializeField, Min(0f)]
    private float decreaseDuration = 0.3f;

    [SerializeField]
    private Ease decreaseEase = Ease.OutQuad;

    private BarGeometry fillGeometry;
    private BarGeometry decreaseGeometry;
    private DOGetter<float> decreaseRatioGetter;
    private DOSetter<float> decreaseRatioSetter;
    private TweenCallback decreaseTweenKilledCallback;
    private Tween decreaseTween;
    private float displayedFillRatio = -1f;
    private float displayedDecreaseRatio = 1f;

    private void Awake()
    {
        if (targetEnemy == null)
            targetEnemy = GetComponentInParent<Enemy>();

        decreaseRatioGetter = GetDisplayedDecreaseRatio;
        decreaseRatioSetter = SetDisplayedDecreaseRatio;
        decreaseTweenKilledCallback = ClearDecreaseTween;
        CacheBarGeometry();
    }

    private void OnEnable()
    {
        displayedFillRatio = -1f;
        CacheBarGeometry();

        if (targetEnemy != null)
            targetEnemy.OnDamageTaken += HandleDamageTaken;

        RefreshBarsImmediately();
    }

    private void OnDisable()
    {
        if (targetEnemy != null)
            targetEnemy.OnDamageTaken -= HandleDamageTaken;

        decreaseTween?.Kill();
        decreaseTween = null;
    }

    private void LateUpdate()
    {
        if (targetEnemy == null)
        {
            SetRenderersVisible(false);
            enabled = false;
            return;
        }

        RefreshFill();
    }

    /// <summary>
    /// 자연 회복과 최대 체력 변경은 실제 체력인 Fill에만 반영합니다.
    /// HealthDecreaseBar는 피해 이벤트에서만 변경하여 회복 프레임이 피해 연출을 덮어쓰지 않게 합니다.
    /// </summary>
    private void RefreshFill()
    {
        if (!CanDisplayHealth())
            return;

        float ratio = CalculateHpRatio(targetEnemy.Hp);

        if (Mathf.Approximately(displayedFillRatio, ratio))
            return;

        displayedFillRatio = ratio;
        ApplyRatio(fillRenderer, fillGeometry, ratio);
    }

    private void RefreshBarsImmediately()
    {
        if (!CanDisplayHealth())
            return;

        float ratio = CalculateHpRatio(targetEnemy.Hp);
        displayedFillRatio = ratio;
        displayedDecreaseRatio = ratio;
        ApplyRatio(fillRenderer, fillGeometry, ratio);
        ApplyRatio(healthDecreaseRenderer, decreaseGeometry, ratio);
    }

    /// <summary>
    /// 피해가 발생한 순간 감소 바를 피해 직전 체력으로 맞춘 뒤 피해 직후 체력까지 Tween합니다.
    /// 진행 중인 연출이 있으면 새 피해를 기준으로 즉시 교체하므로 연속 피해도 정확히 표현합니다.
    /// </summary>
    private void HandleDamageTaken(float previousHp, float currentHp)
    {
        if (!CanDisplayHealth())
            return;

        float previousRatio = CalculateHpRatio(previousHp);
        float currentRatio = CalculateHpRatio(currentHp);

        displayedFillRatio = currentRatio;
        ApplyRatio(fillRenderer, fillGeometry, currentRatio);

        decreaseTween?.Kill();
        displayedDecreaseRatio = previousRatio;
        ApplyRatio(healthDecreaseRenderer, decreaseGeometry, previousRatio);

        decreaseTween = DOTween
            .To(decreaseRatioGetter, decreaseRatioSetter, currentRatio, decreaseDuration)
            .SetDelay(decreaseDelay)
            .SetEase(decreaseEase)
            .SetRecyclable(true)
            .SetLink(gameObject, LinkBehaviour.KillOnDisable)
            .OnKill(decreaseTweenKilledCallback);
    }

    private void ClearDecreaseTween()
    {
        decreaseTween = null;
    }

    private float CalculateHpRatio(float hp)
    {
        return targetEnemy.MaxHp > 0f ? Mathf.Clamp01(hp / targetEnemy.MaxHp) : 0f;
    }

    private float GetDisplayedDecreaseRatio()
    {
        return displayedDecreaseRatio;
    }

    private void SetDisplayedDecreaseRatio(float value)
    {
        displayedDecreaseRatio = value;
        ApplyRatio(healthDecreaseRenderer, decreaseGeometry, value);
    }

    private bool CanDisplayHealth()
    {
        return targetEnemy != null
            && fillRenderer != null
            && fillRenderer.sprite != null
            && healthDecreaseRenderer != null
            && healthDecreaseRenderer.sprite != null;
    }

    private void CacheBarGeometry()
    {
        fillGeometry.Cache(fillRenderer);
        decreaseGeometry.Cache(healthDecreaseRenderer);
    }

    private static void ApplyRatio(SpriteRenderer renderer, BarGeometry geometry, float ratio)
    {
        Vector3 scale = geometry.InitialLocalScale;
        scale.x *= ratio;
        renderer.transform.localScale = scale;

        Vector3 position = geometry.InitialLocalPosition;
        position.x += geometry.BoundsMinX * (geometry.InitialLocalScale.x - scale.x);
        renderer.transform.localPosition = position;
    }

    private void SetRenderersVisible(bool isVisible)
    {
        if (backgroundRenderer != null)
            backgroundRenderer.enabled = isVisible;

        if (healthDecreaseRenderer != null)
            healthDecreaseRenderer.enabled = isVisible;

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

        ValidateRenderer(backgroundRenderer, "Background");
        ValidateRenderer(healthDecreaseRenderer, "Health Decrease");
        ValidateRenderer(fillRenderer, "Fill");
    }

    private void ValidateRenderer(SpriteRenderer renderer, string rendererName)
    {
        if (renderer == null)
        {
            Debug.LogError(
                $"[EnemyHealthBar] {rendererName} Renderer가 할당되지 않았습니다.",
                this
            );
            return;
        }

        if (renderer.sprite == null)
            Debug.LogError($"[EnemyHealthBar] {rendererName} Renderer에 Sprite가 없습니다.", this);
    }

    private struct BarGeometry
    {
        public Vector3 InitialLocalPosition { get; private set; }
        public Vector3 InitialLocalScale { get; private set; }
        public float BoundsMinX { get; private set; }

        private bool IsCached { get; set; }

        public void Cache(SpriteRenderer renderer)
        {
            if (IsCached || renderer == null || renderer.sprite == null)
                return;

            InitialLocalPosition = renderer.transform.localPosition;
            InitialLocalScale = renderer.transform.localScale;
            BoundsMinX = renderer.sprite.bounds.min.x;
            IsCached = true;
        }
    }
}
