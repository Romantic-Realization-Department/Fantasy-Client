using UnityEngine;

// SceneName에 대한 참조를 한 곳으로 모으기 위한 클래스
[CreateAssetMenu(fileName = "SceneNameRef", menuName = "ScriptableObjects/SceneNameRef")]
public class SceneNameRef : ScriptableObject
{
    [field: SerializeField]
    public string SceneName { get; private set; }
}
