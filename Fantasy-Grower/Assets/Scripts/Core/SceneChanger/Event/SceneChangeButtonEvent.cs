using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SceneChangeButtonEvent : MonoBehaviour
{
    [SerializeField]
    private SceneNameRef _sceneName;

    [SerializeField]
    private SceneChangeType _type;

    public void LoadScene()
    {
        SceneChanger.LoadScene(_sceneName.SceneName, _type);
    }
}
