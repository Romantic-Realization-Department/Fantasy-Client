using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 현재 선택된 플레이어 직업에 대응하는 프로필 Sprite를 Image에 표시합니다.
/// 직업이 변경되면 GameManager의 이벤트를 받아 자동으로 갱신합니다.
/// </summary>
[RequireComponent(typeof(Image))]
public sealed class PlayerProfileImage : MonoBehaviour
{
    private Image profileImage;
    private GameManager gameManager;

    private void Awake()
    {
        profileImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        BindGameManager();
        RefreshProfile();
    }

    private void Start()
    {
        // OnEnable 시점에 GameManager가 아직 생성되지 않은 실행 순서도 지원합니다.
        if (gameManager != null)
            return;

        BindGameManager();
        RefreshProfile();

        if (gameManager == null)
            Debug.LogError("[PlayerProfileImage] GameManager를 찾을 수 없습니다.", this);
    }

    private void OnDisable()
    {
        UnbindGameManager();
    }

    private void BindGameManager()
    {
        GameManager foundGameManager = GameManager.InstanceOrNull;
        if (foundGameManager == gameManager)
            return;

        UnbindGameManager();
        gameManager = foundGameManager;

        if (gameManager != null)
            gameManager.OnSelectedJobChanged += HandleSelectedJobChanged;
    }

    private void UnbindGameManager()
    {
        if (gameManager == null)
            return;

        gameManager.OnSelectedJobChanged -= HandleSelectedJobChanged;
        gameManager = null;
    }

    private void RefreshProfile()
    {
        if (gameManager == null)
            return;

        profileImage.sprite = gameManager.GetCurrentPlayerProfile();
    }

    private void HandleSelectedJobChanged(Career job)
    {
        profileImage.sprite = gameManager.GetPlayerProfile(job);
    }
}
