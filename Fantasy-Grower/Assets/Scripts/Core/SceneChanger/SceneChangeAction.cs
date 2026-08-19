using DG.Tweening;
using UnityEngine;

public abstract class SceneChangeAction : MonoBehaviour
{
    protected virtual void Awake() { }

    /// <summary>
    /// 씬 전환이 이루어지기 전
    /// </summary>
    /// <returns></returns>
    public abstract Tween BeforeChange();

    /// <summary>
    /// 씬 전환이 이루어진 후
    /// </summary>
    /// <returns></returns>
    public abstract Tween AfterChange();
}
