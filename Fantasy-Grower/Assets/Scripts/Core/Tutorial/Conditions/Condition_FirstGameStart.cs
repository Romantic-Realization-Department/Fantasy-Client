using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(
    fileName = "Condition_FirstGameStart",
    menuName = "ScriptableObjects/TutorialConditions/FirstGameStart"
)]
public class Condition_FirstGameStart : TutorialConditionBase
{
    [SerializeField]
    private SceneNameRef sceneNameRef;

    private string curSceneName;

    private bool condition = false;

    private void OnEnable()
    {
        condition = false;
        curSceneName = string.Empty;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneChanger.SceneLoaded += OnTotallySceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        curSceneName = scene.name;
    }

    void OnTotallySceneLoaded()
    {
        if (curSceneName == sceneNameRef.SceneName)
        {
            condition = true;
        }
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneChanger.SceneLoaded -= OnTotallySceneLoaded;
    }

    public override bool CheckCondition() => condition;
}
