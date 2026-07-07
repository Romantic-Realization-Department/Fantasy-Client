using UnityEngine;

/// <summary>
/// 인스펙터에서 기획자가 튜토리얼 스텝들을 조립하는 컴포넌트.
/// 게임 시작 시 이 데이터를 TutorialManager에 전달하여 튜토리얼을 시작한다.
/// </summary>
public class TutorialPreviewer : MonoBehaviour
{
    [Header("Sequence Data")]
    [SerializeField, Tooltip("기획자가 에셋(ScriptableObject) 형태로 만들어둔 튜토리얼 데이터")]
    private TutorialSequenceData sequenceData;

    [Header("Test Options")]
    [
        SerializeField,
        Tooltip("게임 시작 시(Start) 이 튜토리얼 시퀀스를 자동으로 테스트 실행할지 여부")
    ]
    private bool playOnStart = false;

#if UNITY_EDITOR
    [Header("Editor Preview")]
    [SerializeField, Tooltip("에디터 씬(Scene) 뷰에서 레이아웃을 미리 볼 스텝의 인덱스 번호")]
    private int previewStepIndex = 0;

    [SerializeField, Tooltip("기즈모를 그릴 기준이 되는 튜토리얼 오버레이 UI (에디터 전용)")]
    private TutorialUIOverlay previewOverlay;
#endif

    private void OnValidate()
    {
        if (sequenceData == null)
        {
            Debug.LogError(
                $"[TutorialPreviewer] {gameObject.name} 컴포넌트에 TutorialSequenceData 에셋이 할당되지 않았습니다! 인스펙터를 확인해주세요."
            );
        }
#if UNITY_EDITOR
        if (previewOverlay == null)
        {
            Debug.LogError(
                $"[TutorialPreviewer] {gameObject.name} 컴포넌트에 에디터 미리보기를 위한 TutorialUIOverlay가 할당되지 않았습니다!"
            );
        }
#endif
    }

    private void Start()
    {
        // 에디터 테스트용으로만 남겨두며, 실제 런타임 호출은 기능 해금 시점에 외부에서 TutorialManager를 통해 직접 트리거합니다.
        if (playOnStart && sequenceData != null && sequenceData.steps.Count > 0)
        {
            TutorialManager.Instance.StartTutorialSequence(sequenceData.steps);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (sequenceData == null || sequenceData.steps == null || sequenceData.steps.Count == 0)
            return;
        if (previewStepIndex < 0 || previewStepIndex >= sequenceData.steps.Count)
            return;

        TutorialStepData data = sequenceData.steps[previewStepIndex];

        // 에디터 모드에서는 UIRegistry가 동작하지 않으므로 씬 전체에서 직접 탐색
        RectTransform targetRect = null;
        if (!string.IsNullOrEmpty(data.targetUI_ID))
        {
            UIRegistrar[] registrars = FindObjectsByType<UIRegistrar>(FindObjectsSortMode.None);
            foreach (var r in registrars)
            {
                if (r.UI_ID == data.targetUI_ID)
                {
                    targetRect = r.GetComponent<RectTransform>();
                    break;
                }
            }
        }

        // 1. 오버레이 캔버스(가이드 UI가 그려질 기준) 탐색
        TutorialUIOverlay overlay = previewOverlay;
        if (overlay == null)
        {
            overlay = FindAnyObjectByType<TutorialUIOverlay>();
        }
        if (overlay == null)
            return;

        Canvas overlayCanvas = overlay.GetComponentInParent<Canvas>();
        if (overlayCanvas == null)
            return;

        Camera targetCam =
            targetRect != null ? targetRect.GetComponentInParent<Canvas>()?.worldCamera : null;
        Camera overlayCam =
            overlayCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : overlayCanvas.worldCamera;

        Vector2 localCenter = Vector2.zero;
        Vector2 localSize = Vector2.zero;
        Vector2 screenPos = new Vector2(Screen.width / 2f, Screen.height / 2f);

        if (targetRect != null && targetCam != null)
        {
            // 타겟 UI의 월드 코너를 구해서, 오버레이 캔버스의 로컬(픽셀) 좌표로 변환
            Vector3[] corners = new Vector3[4];
            targetRect.GetWorldCorners(corners);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlay.rectTransform,
                RectTransformUtility.WorldToScreenPoint(targetCam, corners[0]),
                overlayCam,
                out Vector2 localMin
            );
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlay.rectTransform,
                RectTransformUtility.WorldToScreenPoint(targetCam, corners[2]),
                overlayCam,
                out Vector2 localMax
            );

            localCenter = (localMin + localMax) * 0.5f;
            localSize = new Vector2(
                Mathf.Abs(localMax.x - localMin.x),
                Mathf.Abs(localMax.y - localMin.y)
            );
            screenPos = RectTransformUtility.WorldToScreenPoint(targetCam, targetRect.position);
        }

        // 🚨 모든 기즈모 표기를 오버레이 캔버스의 로컬 공간에만 강제
        Gizmos.matrix = overlay.rectTransform.localToWorldMatrix;

        // 2. 뚫릴 구멍 위치 시각화 (빨간색) - 타겟이 있을 때만
        if (targetRect != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawWireCube(localCenter, new Vector3(localSize.x, localSize.y, 0));
        }

        // 2. 대화창 예상 위치 시각화 (파란색)
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.5f);

        if (data.useCustomPosition)
        {
            Vector2 finalPanelPos = data.customDialogueAnchoredPosition;
            Vector3 finalPanelSize = new Vector3(
                data.customDialogueSize.x,
                data.customDialogueSize.y,
                0
            );
            Gizmos.DrawWireCube(finalPanelPos, finalPanelSize);
        }
        else
        {
            float screenHalf = Screen.height / 2f;
            Vector2 finalPanelPos =
                localCenter
                + (screenPos.y > screenHalf ? new Vector2(0, -150) : new Vector2(0, 150));
            Gizmos.DrawWireCube(finalPanelPos, new Vector3(400, 200, 0));
        }

        // 3. 화살표 예상 위치 시각화 (노란색)
        if (data.arrowDir != PointerDirection.None)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.8f);
            if (data.useCustomPosition)
            {
                Vector2 finalArrowPos = data.customArrowAnchoredPosition;
                Vector3 finalArrowSize = new Vector3(
                    data.customArrowSize.x,
                    data.customArrowSize.y,
                    0
                );
                Gizmos.DrawWireCube(finalArrowPos, finalArrowSize);
            }
            else
            {
                float distance = 100f;
                Vector2 offset = Vector2.zero;
                switch (data.arrowDir)
                {
                    case PointerDirection.Up:
                        offset = new Vector2(0, -distance);
                        break;
                    case PointerDirection.Down:
                        offset = new Vector2(0, distance);
                        break;
                    case PointerDirection.Left:
                        offset = new Vector2(distance, 0);
                        break;
                    case PointerDirection.Right:
                        offset = new Vector2(-distance, 0);
                        break;
                }
                Vector2 finalArrowPos = localCenter + offset;
                Gizmos.DrawWireCube(finalArrowPos, new Vector3(100, 100, 0));
            }
        }

        // 매트릭스 원상복구
        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}
