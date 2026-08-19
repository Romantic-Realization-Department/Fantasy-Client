using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class GameStartAction : MonoBehaviour
{
    [SerializeField, Header("초회차일 시 띄울 Panel")]
    private GameObject _playerSelectUI;

    [SerializeField, Header("초회차가 아닐 시 넘어갈 Scene")]
    private SceneNameRef _basicDungeon;

    private Button _panel;

    private void Awake()
    {
        _panel = GetComponent<Button>();
        _panel.onClick.AddListener(GameStartOrPlayerSelect);
    }

    private void GameStartOrPlayerSelect()
    {
        if (
            true /*TODO : 초회 차인지 아닌지 구분하는 조건 추가*/
        )
        {
            // 초회차일 때
            _playerSelectUI.SetActive(true);
        }
        else
        {
            // 초회차가 아닐 때
            SceneChanger.LoadScene(_basicDungeon.SceneName, SceneChangeType.PageSwap);
        }
    }

    private void OnValidate()
    {
        if (_playerSelectUI == null)
        {
            Debug.LogError("[GameStartAction] Player Select UI가 지정되지 않았습니다.", this);
        }
        if (_basicDungeon == null)
        {
            Debug.LogError(
                "[GameStartAction] Basic Dungeon SceneNameRef가 지정되지 않았습니다.",
                this
            );
        }
    }
}
