using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class UI_StageSlider : MonoBehaviour
{
    [SerializeField, RequireInterface(typeof(IStageProvider))]
    private UnityEngine.Object stageProvider;
    private IStageProvider StageProvider => stageProvider as IStageProvider;

    [SerializeField]
    private TMP_Text stage;

    [SerializeField]
    private DungeonType type;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.minValue = 1;
        StageProvider.OnStageChanged += OnStageChanged;
    }

    private void OnEnable()
    {
        OnStageChanged(0);
    }

    void OnStageChanged(int stage)
    {
        slider.maxValue = GameManager.InstanceOrNull.GetDungeonRecord(type);
    }

    private void OnDestroy()
    {
        if (StageProvider != null)
        {
            StageProvider.OnStageChanged -= OnStageChanged;
        }
    }

    public void OnValueChanged(float value)
    {
        int valueInt = (int)value;

        stage.SetText("스테이지 {0}", valueInt);
    }
}
