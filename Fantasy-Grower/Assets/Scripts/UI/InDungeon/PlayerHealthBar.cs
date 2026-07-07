using DG.Tweening;
using DG.Tweening.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 던전 UI에 현재 플레이어의 체력과 실제 체력 수치를 표시합니다.
/// Fill은 실제 체력에 즉시 반응하고, HealthDecreaseBar는 피해 직전 비율에서 피해 직후 비율까지 지연되어 따라갑니다.
/// </summary>
[DisallowMultipleComponent]
public sealed class PlayerHealthBar : MonoBehaviour
{
    [Header("체력바 UI")]
    [SerializeField]
    private Image backgroundImage;

    [SerializeField]
    private Image healthDecreaseImage;

    [SerializeField]
    private Image fillImage;

    [SerializeField]
    private TMP_Text healthText;

    [Header("감소 연출")]
    [SerializeField, Min(0f)]
    private float decreaseDelay = 0.15f;

    [SerializeField, Min(0f)]
    private float decreaseDuration = 0.3f;

    [SerializeField]
    private Ease decreaseEase = Ease.OutQuad;

    private Player targetPlayer;
    private DOGetter<float> decreaseRatioGetter;
    private DOSetter<float> decreaseRatioSetter;
    private TweenCallback decreaseTweenKilledCallback;
    private Tween decreaseTween;
    private float displayedDecreaseRatio = 1f;
    private float displayedFillRatio = -1f;
    private float displayedCurrentHp = -1f;
    private float displayedMaxHp = -1f;

    private void Awake()
    {
        decreaseRatioGetter = GetDisplayedDecreaseRatio;
        decreaseRatioSetter = SetDisplayedDecreaseRatio;
        decreaseTweenKilledCallback = ClearDecreaseTween;
    }

    private void OnEnable()
    {
        displayedFillRatio = -1f;
        displayedCurrentHp = -1f;
        displayedMaxHp = -1f;

        if (!TryBindPlayer())
            SetGraphicsVisible(false);
    }

    private void OnDisable()
    {
        UnbindPlayer();
        decreaseTween?.Kill();
        decreaseTween = null;
    }

    private void LateUpdate()
    {
        if (targetPlayer == null && !TryBindPlayer())
            return;

        RefreshFillAndText();
    }

    /// <summary>
    /// PlayerInjectionFeature의 Start 실행 순서와 무관하게 플레이어가 생성된 시점에 연결합니다.
    /// GameManager가 없는 단독 씬 테스트에서는 현재 씬의 Player를 직접 탐색합니다.
    /// </summary>
    private bool TryBindPlayer()
    {
        Player foundPlayer = null;
        GameManager gameManager = GameManager.InstanceOrNull;

        if (gameManager != null)
            foundPlayer = gameManager.GetPlayer() as Player;

        if (foundPlayer == null)
            foundPlayer = FindAnyObjectByType<Player>();

        if (foundPlayer == null)
            return false;

        if (foundPlayer == targetPlayer)
            return true;

        UnbindPlayer();
        targetPlayer = foundPlayer;
        targetPlayer.OnDamageTaken += HandleDamageTaken;
        SetGraphicsVisible(true);
        RefreshBarsImmediately();
        return true;
    }

    private void UnbindPlayer()
    {
        if (targetPlayer == null)
            return;

        targetPlayer.OnDamageTaken -= HandleDamageTaken;
        targetPlayer = null;
    }

    private void RefreshFillAndText()
    {
        if (!CanDisplayHealth())
            return;

        float ratio = CalculateHpRatio(targetPlayer.Hp);
        if (!Mathf.Approximately(displayedFillRatio, ratio))
        {
            displayedFillRatio = ratio;
            fillImage.fillAmount = ratio;
        }

        RefreshHealthText();
    }

    private void RefreshBarsImmediately()
    {
        if (!CanDisplayHealth())
            return;

        float ratio = CalculateHpRatio(targetPlayer.Hp);
        displayedFillRatio = ratio;
        displayedDecreaseRatio = ratio;
        fillImage.fillAmount = ratio;
        healthDecreaseImage.fillAmount = ratio;
        RefreshHealthText();
    }

    private void RefreshHealthText()
    {
        float currentHp = Mathf.Ceil(targetPlayer.Hp);
        float maxHp = Mathf.Ceil(targetPlayer.MaxHp);

        if (
            Mathf.Approximately(displayedCurrentHp, currentHp)
            && Mathf.Approximately(displayedMaxHp, maxHp)
        )
        {
            return;
        }

        displayedCurrentHp = currentHp;
        displayedMaxHp = maxHp;
        healthText.SetText("{0:0}/{1:0}", currentHp, maxHp);
    }

    private void HandleDamageTaken(float previousHp, float currentHp)
    {
        if (!CanDisplayHealth())
            return;

        float previousRatio = CalculateHpRatio(previousHp);
        float currentRatio = CalculateHpRatio(currentHp);

        displayedFillRatio = currentRatio;
        fillImage.fillAmount = currentRatio;
        RefreshHealthText();

        decreaseTween?.Kill();
        displayedDecreaseRatio = previousRatio;
        healthDecreaseImage.fillAmount = previousRatio;

        decreaseTween = DOTween
            .To(decreaseRatioGetter, decreaseRatioSetter, currentRatio, decreaseDuration)
            .SetDelay(decreaseDelay)
            .SetEase(decreaseEase)
            .SetRecyclable(true)
            .SetLink(gameObject, LinkBehaviour.KillOnDisable)
            .OnKill(decreaseTweenKilledCallback);
    }

    private float CalculateHpRatio(float hp)
    {
        return targetPlayer.MaxHp > 0f ? Mathf.Clamp01(hp / targetPlayer.MaxHp) : 0f;
    }

    private float GetDisplayedDecreaseRatio()
    {
        return displayedDecreaseRatio;
    }

    private void SetDisplayedDecreaseRatio(float value)
    {
        displayedDecreaseRatio = value;
        healthDecreaseImage.fillAmount = value;
    }

    private void ClearDecreaseTween()
    {
        decreaseTween = null;
    }

    private bool CanDisplayHealth()
    {
        return targetPlayer != null
            && backgroundImage != null
            && healthDecreaseImage != null
            && fillImage != null
            && healthText != null;
    }

    private void SetGraphicsVisible(bool isVisible)
    {
        if (backgroundImage != null)
            backgroundImage.enabled = isVisible;

        if (healthDecreaseImage != null)
            healthDecreaseImage.enabled = isVisible;

        if (fillImage != null)
            fillImage.enabled = isVisible;

        if (healthText != null)
            healthText.enabled = isVisible;
    }

    private void OnValidate()
    {
        ValidateImage(backgroundImage, "Background");
        ValidateFilledImage(healthDecreaseImage, "Health Decrease");
        ValidateFilledImage(fillImage, "Fill");

        if (healthText == null)
            Debug.LogError("[PlayerHealthBar] Health Text가 할당되지 않았습니다.", this);
    }

    private void ValidateImage(Image image, string imageName)
    {
        if (image == null)
            Debug.LogError($"[PlayerHealthBar] {imageName} Image가 할당되지 않았습니다.", this);
    }

    private void ValidateFilledImage(Image image, string imageName)
    {
        ValidateImage(image, imageName);
        if (image == null)
            return;

        if (image.type != Image.Type.Filled)
        {
            Debug.LogError(
                $"[PlayerHealthBar] {imageName} Image Type을 Filled로 설정해주세요.",
                this
            );
            return;
        }

        if (image.fillMethod != Image.FillMethod.Horizontal)
            Debug.LogError(
                $"[PlayerHealthBar] {imageName} Fill Method를 Horizontal로 설정해주세요.",
                this
            );

        if (image.fillOrigin != (int)Image.OriginHorizontal.Left)
            Debug.LogError(
                $"[PlayerHealthBar] {imageName} Fill Origin을 Left로 설정해주세요.",
                this
            );
    }
}
