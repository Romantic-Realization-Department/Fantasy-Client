using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneChangeType
{
    FadeInOut,
    PageSwap,
}

public class SceneChanger : MonoBehaviour
{
    private static SceneChanger _instance;

    [Serializable]
    public struct TypeToAction
    {
        public SceneChangeType Key;
        public SceneChangeAction Value;
    }

    [SerializeField]
    private TypeToAction[] _sceneChangePairs;

    private readonly Dictionary<SceneChangeType, SceneChangeAction> _sceneChangeDic = new();

    private SceneChangeAction _currentSceneChangeAction;

    /// <summary>
    /// SceneChange 연출이 완전히 끝난 직후
    /// </summary>
    public static event Action SceneLoaded;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        gameObject.SetActive(true);

        // Dictionary 초기화
        foreach (var pair in _sceneChangePairs)
            _sceneChangeDic[pair.Key] = pair.Value;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 로드된 후, 현재 진행 중인 씬 전환 액션이 있다면 AfterChange를 실행하고, 그렇지 않다면 방금 실행한 상태이므로 바로 SceneLoaded 이벤트를 발생시킵니다.
        if (!_currentSceneChangeAction)
        {
            SceneLoaded?.Invoke(); // 첫 씬이 로드된 후 바로 이벤트를 발생시킴
            return;
        }

        _currentSceneChangeAction.AfterChange().OnComplete(() => SceneLoaded?.Invoke());
        _currentSceneChangeAction = null;
    }

    public static void LoadScene(string sceneName, SceneChangeType type)
    {
        // 이미 실행중인 LoadScene이 존재할 때
        if (_instance._currentSceneChangeAction != null)
            return;

        // 사전에 준비된 액션인지 체크
        if (!_instance._sceneChangeDic.TryGetValue(type, out SceneChangeAction sceneChangeAction))
            return;

        _instance._currentSceneChangeAction = sceneChangeAction;
        sceneChangeAction.BeforeChange().onComplete += () => SceneManager.LoadScene(sceneName);
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
