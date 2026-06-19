using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 플레이어(직업) 선택 화면. 각 직업 버튼을 GameManager.SelectJob에 연결하고,
/// 선택 확정 시 다음 씬으로 전환한다.
///
/// 에디터 설정:
///  - jobButtons 배열에 직업과 버튼/하이라이트를 매핑한다.
///  - confirmButton, nextSceneName을 지정하면 확정 시 씬을 로드한다.
/// </summary>
public class PlayerSelectUI : MonoBehaviour
{
    [Serializable]
    private struct JobButton
    {
        public Career job;
        public Button button;
        public GameObject highlight; // 선택 표시용(선택). 비워둬도 동작
    }

    [Header("직업 버튼")]
    [SerializeField] private JobButton[] jobButtons;

    [Header("확정")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private string nextSceneName;

    private Career selectedJob;

    private void Start()
    {
        // GameManager의 현재 선택값으로 초기화
        var gm = GameManager.InstanceOrNull;
        selectedJob = gm != null ? gm.SelectedJob : Career.Warrior;

        foreach (var entry in jobButtons)
        {
            if (entry.button == null) continue;
            Career job = entry.job; // 클로저 캡처용 지역 변수
            entry.button.onClick.AddListener(() => OnJobSelected(job));
        }

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);

        RefreshHighlight();
    }

    /// <summary>직업 버튼 클릭 시 선택값을 갱신한다.</summary>
    private void OnJobSelected(Career job)
    {
        selectedJob = job;
        RefreshHighlight();
    }

    /// <summary>
    /// 선택을 GameManager에 반영하고 다음 씬으로 전환한다.
    /// </summary>
    private void OnConfirm()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.SelectJob(selectedJob);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    private void RefreshHighlight()
    {
        foreach (var entry in jobButtons)
        {
            if (entry.highlight != null)
                entry.highlight.SetActive(entry.job == selectedJob);
        }
    }
}
