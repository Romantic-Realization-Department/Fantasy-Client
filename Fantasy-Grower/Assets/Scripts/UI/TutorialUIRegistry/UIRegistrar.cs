using UnityEngine;

/// <summary>
/// 이 컴포넌트를 UI에 부착하고 ID를 지정하면 Awake 시점에 UIRegistry에 자동 등록된다.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class UIRegistrar : MonoBehaviour
{
    [SerializeField, Tooltip("UIRegistry에 등록될 고유 문자열 ID (예: Btn_Dungeon)")]
    private UIKeyRegistry uiID;

    public string UI_ID => uiID;

    private void Awake()
    {
        if (!string.IsNullOrEmpty(uiID))
        {
            UIRegistry.Register(uiID, transform as RectTransform);
        }
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(uiID))
        {
            UIRegistry.Unregister(uiID);
        }
    }
}
