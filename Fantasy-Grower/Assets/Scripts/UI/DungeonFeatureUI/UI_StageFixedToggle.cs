using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public sealed class UI_StageFixedToggle : MonoBehaviour
{
    [SerializeField, RequireInterface(typeof(IStageProvider))]
    private UnityEngine.Object stageProviderObject;

    [SerializeField]
    private TMP_Text labelText;

    [SerializeField]
    private Image targetImage;

    [SerializeField]
    private Sprite fixedSprite;

    [SerializeField]
    private Sprite autoAdvanceSprite;

    [SerializeField]
    private string fixedLabel = "스테이지 고정";

    [SerializeField]
    private string autoAdvanceLabel = "자동 진행";

    private IStageProvider stageProvider;
    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        if (targetImage == null && toggle.targetGraphic is Image targetGraphicImage)
            targetImage = targetGraphicImage;

        stageProvider = stageProviderObject as IStageProvider;

        if (stageProvider == null && stageProviderObject != null)
        {
            Debug.LogError(
                $"[UI_StageFixedToggle] 할당된 오브젝트({stageProviderObject.name})가 IStageProvider를 구현하지 않았습니다.",
                this
            );
        }
    }

    private void OnEnable()
    {
        if (toggle != null)
            toggle.onValueChanged.AddListener(HandleToggleValueChanged);

        if (stageProvider != null)
        {
            stageProvider.OnStageFixedChanged += HandleStageFixedChanged;
            SyncToggle(stageProvider.IsStageFixed);
        }
        else
        {
            UpdateView(false);
        }
    }

    private void OnDisable()
    {
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(HandleToggleValueChanged);

        if (stageProvider != null)
            stageProvider.OnStageFixedChanged -= HandleStageFixedChanged;
    }

    private void HandleToggleValueChanged(bool isOn)
    {
        if (stageProvider != null)
            stageProvider.SetStageFixed(isOn);

        UpdateView(isOn);
    }

    private void HandleStageFixedChanged(bool isFixed)
    {
        SyncToggle(isFixed);
    }

    private void SyncToggle(bool isFixed)
    {
        if (toggle != null)
            toggle.SetIsOnWithoutNotify(isFixed);

        UpdateView(isFixed);
    }

    private void UpdateView(bool isFixed)
    {
        if (labelText != null)
            labelText.text = isFixed ? fixedLabel : autoAdvanceLabel;

        if (targetImage != null)
            targetImage.sprite = isFixed ? fixedSprite : autoAdvanceSprite;
    }
}
