using UnityEngine;

public class DungeonSelect : MonoBehaviour
{
    [SerializeField]
    private DungeonSelectButton[] _dungeonSelectButtons;

    private void Awake()
    {
        foreach (var d in _dungeonSelectButtons)
        {
            d.SelectButton.onClick.AddListener(() => OnClick(d));
        }
    }

    private void OnClick(DungeonSelectButton dungeonSelectButton)
    {
        if (dungeonSelectButton.Filter.activeSelf)
        {
            // 누른 대상만 Select상태로 변경
            foreach (var d in _dungeonSelectButtons)
            {
                if (d == dungeonSelectButton)
                {
                    dungeonSelectButton.Select();
                }
                else
                {
                    d.Return();
                }
            }
        }
        else
        {
            // Select상태로 누르면 씬 이동
            dungeonSelectButton.MovingScene();
        }
    }

    private void OnValidate()
    {
        if (_dungeonSelectButtons == null || _dungeonSelectButtons.Length == 0)
        {
            Debug.Log("DungeonSelectOnUI가 할당되지 않았습니다!!!", this);
        }
    }
}
