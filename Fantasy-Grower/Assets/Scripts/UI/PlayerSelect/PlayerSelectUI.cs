using System;
using DG.Tweening; // DOTween 사용
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 플레이어(직업) 선택 화면. 각 직업 버튼을 눌러 선택하고, 확정 시 다음 씬으로 전환합니다.
/// 추가됨: 선택된 UI 크기 펌핑 애니메이션(DOTween), 확정 시 재확인 팝업(ConfirmPanel) 로직
/// </summary>
public class PlayerSelectUI : MonoBehaviour
{
    [Serializable]
    private struct JobButton
    {
        public Career job;
        public Button button;

        [HideInInspector]
        public Vector2 originalSize; // 원본 크기 캐싱용
    }

    [Header("직업 버튼")]
    [SerializeField]
    private JobButton[] jobButtons;

    [Header("확정 버튼")]
    [SerializeField]
    private Button confirmButton;

    [Header("재확인 팝업 패널")]
    [SerializeField]
    private GameObject confirmPanel;

    [SerializeField]
    private Button panelYesButton;

    [SerializeField]
    private Button panelNoButton;

    [SerializeField]
    private SceneNameRef nextSceneNameRef;

    [Header("선택/미선택 밝기 (색상 조절)")]
    [SerializeField]
    private Color _selectedColor = Color.white; // 밝게 (원본 색상)

    [SerializeField]
    private Color _unselectedColor = Color.gray; // 어둡게 (명도 50%)

    // 아무것도 선택되지 않은 상태를 지원하기 위해 Nullable 타입(Career?) 사용
    private Career? _selectedJob = null;

    private void Start()
    {
        // 시작 시 아무것도 선택되지 않은 상태로 초기화
        _selectedJob = null;

        // 재확인 패널은 처음에 무조건 꺼둡니다
        if (confirmPanel != null)
            confirmPanel.SetActive(false);

        if (jobButtons != null)
        {
            for (int i = 0; i < jobButtons.Length; i++)
            {
                if (jobButtons[i].button == null)
                    continue;

                // 원본 SizeDelta 캐싱 (픽셀 깨짐 방지를 위한 기준값)
                jobButtons[i].originalSize = (
                    (RectTransform)jobButtons[i].button.transform
                ).sizeDelta;

                Career job = jobButtons[i].job;
                jobButtons[i].button.onClick.AddListener(() => OnJobSelected(job));
            }
        }

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);

        // 팝업 버튼 이벤트 연결
        if (panelYesButton != null)
            panelYesButton.onClick.AddListener(OnConfirmYes);
        if (panelNoButton != null)
            panelNoButton.onClick.AddListener(OnConfirmNo);

        // 초기 색상 및 상태 세팅 (최초 시작 시에는 애니메이션 없이 즉시 세팅)
        RefreshHighlight(true);
    }

    /// <summary>직업 버튼 클릭 시 선택값을 갱신한다.</summary>
    private void OnJobSelected(Career job)
    {
        // 이미 선택된 직업을 또 누르면 무시 (불필요한 DOTween 재생 방지)
        if (_selectedJob != null && _selectedJob.Value == job)
            return;

        _selectedJob = job;
        RefreshHighlight(false); // 애니메이션 있게 갱신
    }

    /// <summary>
    /// 첫 번째 확정 버튼 클릭 시 호출 (팝업 띄우기)
    /// </summary>
    private void OnConfirmClicked()
    {
        if (_selectedJob == null)
        {
            Debug.LogWarning("직업을 먼저 선택해 주세요!");
            return;
        }

        // 재확인 팝업이 할당되어 있다면 띄우고, 없으면 바로 확정 처리
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(true);
        }
        else
        {
            // 인스펙터에 패널 할당을 깜빡했을 때를 대비한 안전 장치
            OnConfirmYes();
        }
    }

    /// <summary>
    /// 재확인 팝업에서 "예"를 눌렀거나, 팝업이 없을 때 실제 확정 처리
    /// </summary>
    private void OnConfirmYes()
    {
        var gm = GameManager.InstanceOrNull;
        if (gm != null)
        {
            gm.SelectJob(_selectedJob.Value);
        }

        if (nextSceneNameRef != null && !string.IsNullOrEmpty(nextSceneNameRef.SceneName))
            SceneChanger.LoadScene(nextSceneNameRef.SceneName, SceneChangeType.PageSwap);
    }

    /// <summary>
    /// 재확인 팝업에서 "아니오"를 눌렀을 때
    /// </summary>
    private void OnConfirmNo()
    {
        if (confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
    }

    private void RefreshHighlight(bool isInstant)
    {
        if (jobButtons == null)
            return;

        bool hasSelection = _selectedJob != null;

        // 선택된 직업이 있을 때만 Confirm 버튼을 화면에 띄웁니다.
        if (confirmButton != null)
        {
            confirmButton.gameObject.SetActive(hasSelection);
        }

        foreach (var entry in jobButtons)
        {
            if (entry.button == null)
                continue;

            // 선택 여부 판별
            bool isSelected = (hasSelection && entry.job == _selectedJob.Value);

            // 1. 색상(밝기) 조절 로직
            Graphic targetGraphic = entry.button.targetGraphic;
            if (targetGraphic != null)
            {
                targetGraphic.color = isSelected ? _selectedColor : _unselectedColor;
            }

            // 2. 크기 조절 로직 (DOSizeDelta)
            // 픽셀 아트 왜곡을 막기 위해 원본 사이즈에 배율을 곱해서 적용
            RectTransform rect = (RectTransform)entry.button.transform;
            Vector2 targetSize = isSelected ? entry.originalSize * 1.1f : entry.originalSize;

            if (isInstant)
            {
                rect.sizeDelta = targetSize;
            }
            else
            {
                // 통통 튀는 느낌(OutBack)으로 부드럽게 너비/높이 변경
                rect.DOKill(); // 기존 실행 중인 애니메이션 취소
                rect.DOSizeDelta(targetSize, 0.2f).SetEase(Ease.OutBack).SetRecyclable(true);
            }
        }
    }
}
