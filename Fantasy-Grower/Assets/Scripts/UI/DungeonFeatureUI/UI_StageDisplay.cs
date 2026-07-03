using TMPro;
using UnityEngine;

/// <summary>
/// 스테이지 정보를 구독하여 UI 텍스트에 표시하는 컴포넌트입니다.
/// </summary>
public class UI_StageDisplay : MonoBehaviour
{
    [SerializeField, Tooltip("스테이지 숫자를 표시할 텍스트 컴포넌트")]
    private TextMeshProUGUI _stageText;

    [
        SerializeField,
        RequireInterface(typeof(IStageProvider)),
        Tooltip("IStageProvider를 구현한 스테이지 매니저")
    ]
    private UnityEngine.Object _stageProviderObj;

    private IStageProvider _stageProvider;

    private void OnValidate()
    {
        if (_stageText == null)
            Debug.LogError(
                $"[{gameObject.name} - UI_StageDisplay] _stageText가 할당되지 않았습니다!"
            );
    }

    private void Awake()
    {
        _stageProvider = _stageProviderObj as IStageProvider;

        if (_stageProvider == null && _stageProviderObj != null)
        {
            Debug.LogError(
                $"[UI_StageDisplay] 할당된 오브젝트({_stageProviderObj.name})가 IStageProvider를 구현하지 않았습니다!"
            );
        }
    }

    private void OnEnable()
    {
        if (_stageProvider != null)
        {
            _stageProvider.OnStageChanged += UpdateStageUI;

            UpdateStageUI(_stageProvider.CurrentStageIndex);
        }
    }

    private void OnDisable()
    {
        if (_stageProvider != null)
        {
            _stageProvider.OnStageChanged -= UpdateStageUI;
        }
    }

    private void UpdateStageUI(int stageIndex)
    {
        if (_stageText != null)
        {
            // string 할당(GC)을 피하는 TMPro 전용 최적화 함수 사용
            _stageText.SetText("스테이지 {0}", stageIndex + 1);
        }
    }
}
