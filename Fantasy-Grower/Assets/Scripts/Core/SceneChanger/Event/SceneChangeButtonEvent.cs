using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SceneChangeButtonEvent : MonoBehaviour
{
    private Button _button;

    [SerializeField]
    private string _sceneName;

    [SerializeField]
    private SceneChangeType _type;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() => SceneChanger.LoadScene(_sceneName, _type));
    }
}
