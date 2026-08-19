using UnityEngine;

[CreateAssetMenu(fileName = "DefaultUIEvent", menuName = "DefaultUIEvent")]
public class DefaultUIEvent : ScriptableObject
{
    public void EnableObject(GameObject go)
    {
        go.SetActive(true);
    }

    public void DisableObject(GameObject go)
    {
        go.SetActive(false);
    }
}
