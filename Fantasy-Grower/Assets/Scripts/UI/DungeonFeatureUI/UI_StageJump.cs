using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 슬라이더의 값(value)을 읽어와서 특정 스테이지로 점프하는 UI 컴포넌트입니다.
/// </summary>
public class UI_StageJump : MonoBehaviour
{
    [SerializeField, Tooltip("스테이지 번호를 선택할 슬라이더")]
    private Slider _stageSlider;

    [SerializeField, Tooltip("슬라이더의 값으로 이동하는 버튼")]
    private Button _jumpButton;

    [
        SerializeField,
        RequireInterface(typeof(IStageProvider)),
        Tooltip("IStageProvider를 구현한 스테이지 매니저")
    ]
    private UnityEngine.Object _stageProviderObj;

    private IStageProvider _stageProvider;

    private void OnValidate()
    {
        if (_stageSlider == null)
            Debug.LogError(
                $"[{gameObject.name} - UI_StageJump] _stageSlider가 할당되지 않았습니다!"
            );

        if (_jumpButton == null)
            Debug.LogError(
                $"[{gameObject.name} - UI_StageJump] _jumpButton이 할당되지 않았습니다!"
            );
    }

    private void Awake()
    {
        _stageProvider = _stageProviderObj as IStageProvider;

        if (_stageProvider == null && _stageProviderObj != null)
        {
            Debug.LogError(
                $"[UI_StageJump] 할당된 오브젝트({_stageProviderObj.name})가 IStageProvider를 구현하지 않았습니다!"
            );
        }
    }

    private void Start()
    {
        if (_jumpButton != null)
        {
            _jumpButton.onClick.AddListener(OnJumpButtonClicked);
        }
    }

    private void OnJumpButtonClicked()
    {
        if (_stageProvider != null && _stageSlider != null)
        {
            // 슬라이더의 value를 읽어와서 정수(인덱스)로 변환
            int targetIndex = Mathf.RoundToInt(_stageSlider.value) - 1;

            // 이전에 만들어둔 JumpToStage 호출
            _stageProvider.JumpToStage(targetIndex);

            Debug.Log($"[UI_StageJump] 이동 호출됨: 인덱스 {targetIndex}");
        }
    }

    private void OnDestroy()
    {
        if (_jumpButton != null)
        {
            _jumpButton.onClick.RemoveListener(OnJumpButtonClicked);
        }
    }
}
