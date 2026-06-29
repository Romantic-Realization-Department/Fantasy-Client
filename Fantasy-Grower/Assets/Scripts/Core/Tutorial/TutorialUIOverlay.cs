using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 화면 전체를 덮고, 특정 타겟(RectTransform) 영역만 구멍(Hole)을 뚫어
/// 터치를 통과시키고 시각적으로 강조하는 커스텀 UI 마스킹 및 가이드 컴포넌트.
/// 서로 다른 RenderMode(Overlay vs Camera)를 가진 캔버스 간의 좌표계 변환을 완벽하게 지원합니다.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class TutorialUIOverlay : Graphic, ICanvasRaycastFilter, IPointerClickHandler
{
    public event Action OnOverlayClicked;
    private static TutorialUIOverlay _instance;
    public static TutorialUIOverlay Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindAnyObjectByType<TutorialUIOverlay>();
            return _instance;
        }
    }

    [Header("마스킹 타겟")]
    [SerializeField, Tooltip("구멍을 뚫을 대상 UI 엘리먼트 (런타임 자동 할당)")]
    private RectTransform targetRect;

    [Header("가이드 UI 요소")]
    [SerializeField]
    private GameObject dialoguePanel;

    [SerializeField]
    private TextMeshProUGUI guideText;

    [SerializeField]
    private RectTransform pointerArrow;

    private Tween textTween;
    private TutorialActionType currentActionType;

    protected override void Awake()
    {
        base.Awake();
        _instance = this;
        HideGuide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnOverlayClicked?.Invoke();
    }

    /// <summary>
    /// 대상 UI 캔버스의 카메라를 가져옵니다 (Overlay일 경우 null 반환)
    /// </summary>
    private Camera GetCanvasCamera(RectTransform rect)
    {
        if (rect == null)
            return null;
        Canvas canvas = rect.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            return canvas.worldCamera;
        }
        return null;
    }

    /// <summary>
    /// 타겟 영역 구멍 뚫기, 대화창 띄우기, 화살표 배치를 한 번에 수행한다.
    /// </summary>
    public void ShowGuide(RectTransform target, TutorialStepData data)
    {
        enabled = true; // 가이드 시작 시 활성화하여 렌더링 및 터치 블락 재개
        targetRect = target;
        currentActionType = data.actionType;
        SetAllDirty();

        Camera targetCam = GetCanvasCamera(targetRect);
        Camera overlayCam = GetCanvasCamera(rectTransform);

        // 🚨 핵심 수정: Target UI의 피벗에 흔들리지 않도록 완벽한 기하학적 중심점(Center)을 구함
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;
        Vector2 targetScreenPos = RectTransformUtility.WorldToScreenPoint(targetCam, worldCenter);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            targetScreenPos,
            overlayCam,
            out Vector2 localCenter
        );

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(!string.IsNullOrEmpty(data.dialogueText));
            if (guideText != null)
            {
                textTween?.Kill();

                // GC 최적화: DOText(매 프레임 string 생성) 대신 maxVisibleCharacters 활용
                guideText.text = data.dialogueText;
                guideText.maxVisibleCharacters = 0;

                // 글자 수에 비례하여 애니메이션 시간 결정 (글자당 0.05초)
                float duration = data.dialogueText.Length * 0.05f;

                textTween = DOTween
                    .To(
                        () => guideText.maxVisibleCharacters,
                        x => guideText.maxVisibleCharacters = x,
                        data.dialogueText.Length,
                        duration
                    )
                    .SetEase(Ease.Linear)
                    .SetUpdate(true); // timeScale = 0 환경에서도 정상 동작하도록 설정
            }

            if (target != null)
            {
                var panelRect = dialoguePanel.GetComponent<RectTransform>();

                if (data.useCustomPosition)
                {
                    // 커스텀 위치 사용 시
                    panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                    panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                    panelRect.pivot = new Vector2(0.5f, 0.5f);

                    panelRect.anchoredPosition = data.customDialogueAnchoredPosition;
                    panelRect.sizeDelta = data.customDialogueSize;
                }
                else
                {
                    // 기존 자동 회피 로직
                    float screenHalf = Screen.height / 2f;

                    if (targetScreenPos.y > screenHalf)
                    {
                        panelRect.anchorMin = new Vector2(0.5f, 0);
                        panelRect.anchorMax = new Vector2(0.5f, 0);
                        panelRect.pivot = new Vector2(0.5f, 0);
                        panelRect.anchoredPosition = new Vector2(0, 150);
                    }
                    else
                    {
                        panelRect.anchorMin = new Vector2(0.5f, 1);
                        panelRect.anchorMax = new Vector2(0.5f, 1);
                        panelRect.pivot = new Vector2(0.5f, 1);
                        panelRect.anchoredPosition = new Vector2(0, -150);
                    }
                }
            }
        }

        if (pointerArrow != null && data.arrowDir != PointerDirection.None)
        {
            pointerArrow.gameObject.SetActive(true);

            pointerArrow.anchorMin = new Vector2(0.5f, 0.5f);
            pointerArrow.anchorMax = new Vector2(0.5f, 0.5f);
            pointerArrow.pivot = new Vector2(0.5f, 0.5f);

            if (data.useCustomPosition)
            {
                pointerArrow.localRotation = Quaternion.Euler(
                    0,
                    0,
                    GetRotationFromDirection(data.arrowDir)
                );
                pointerArrow.anchoredPosition = data.customArrowAnchoredPosition;
                pointerArrow.sizeDelta = data.customArrowSize;
            }
            else
            {
                pointerArrow.anchoredPosition = localCenter;
                float distance = 100f;
                Vector2 offset = Vector2.zero;

                pointerArrow.localRotation = Quaternion.Euler(
                    0,
                    0,
                    GetRotationFromDirection(data.arrowDir)
                );

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
                pointerArrow.anchoredPosition += offset;
            }
        }
        else if (pointerArrow != null)
        {
            pointerArrow.gameObject.SetActive(false);
        }
    }

    public void HideGuide()
    {
        targetRect = null;
        SetAllDirty();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (pointerArrow != null)
            pointerArrow.gameObject.SetActive(false);

        // 더 이상 표시할 스텝이 없으면 컴포넌트 자체를 꺼서
        // 화면 전체를 덮는 Quad 렌더링을 중단하고 터치 블락을 해제한다.
        enabled = false;
    }

    private float GetRotationFromDirection(PointerDirection dir) =>
        dir switch
        {
            PointerDirection.Up => 0f,
            PointerDirection.Down => 180f,
            PointerDirection.Left => 90f,
            PointerDirection.Right => -90f,
            _ => 0f,
        };

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (targetRect == null)
            return true;

        // DIALOGUE_ONLY일 경우, 구멍은 뚫리지만 터치는 통과하면 안 됨 (오버레이 클릭으로 진행해야 함)
        if (currentActionType == TutorialActionType.DIALOGUE_ONLY)
            return true;

        // 터치 지점(Screen Point)이 타겟 UI 영역 내부인지 검사할 때는 타겟의 카메라를 기준으로 판별
        Camera targetCam = GetCanvasCamera(targetRect);
        return !RectTransformUtility.RectangleContainsScreenPoint(targetRect, sp, targetCam);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rootRect = rectTransform.rect;

        if (targetRect == null)
        {
            AddQuad(vh, rootRect.min, rootRect.max);
            return;
        }

        Camera targetCam = GetCanvasCamera(targetRect);
        Camera overlayCam = GetCanvasCamera(rectTransform);

        Vector3[] corners = new Vector3[4];
        targetRect.GetWorldCorners(corners);

        // 타겟의 월드 좌표를 스크린 좌표로 변환한 뒤, 현재 오버레이 캔버스의 로컬 좌표로 재변환 (교차 캔버스 지원)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            RectTransformUtility.WorldToScreenPoint(targetCam, corners[0]),
            overlayCam,
            out Vector2 localMin
        );
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            RectTransformUtility.WorldToScreenPoint(targetCam, corners[2]),
            overlayCam,
            out Vector2 localMax
        );

        AddQuad(
            vh,
            new Vector2(rootRect.xMin, localMax.y),
            new Vector2(rootRect.xMax, rootRect.yMax)
        );
        AddQuad(
            vh,
            new Vector2(rootRect.xMin, rootRect.yMin),
            new Vector2(rootRect.xMax, localMin.y)
        );
        AddQuad(vh, new Vector2(rootRect.xMin, localMin.y), new Vector2(localMin.x, localMax.y));
        AddQuad(vh, new Vector2(localMax.x, localMin.y), new Vector2(rootRect.xMax, localMax.y));
    }

    private void AddQuad(VertexHelper vh, Vector2 min, Vector2 max)
    {
        UIVertex vert = UIVertex.simpleVert;
        vert.color = color;
        int startIndex = vh.currentVertCount;

        vert.position = new Vector3(min.x, min.y);
        vh.AddVert(vert);
        vert.position = new Vector3(min.x, max.y);
        vh.AddVert(vert);
        vert.position = new Vector3(max.x, max.y);
        vh.AddVert(vert);
        vert.position = new Vector3(max.x, min.y);
        vh.AddVert(vert);

        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
    }
}
