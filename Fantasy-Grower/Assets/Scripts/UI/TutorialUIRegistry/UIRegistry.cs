using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 씬 내의 주요 UI 엘리먼트들을 문자열 ID로 전역 관리하는 레지스트리.
/// 튜토리얼 등 외부 시스템이 씬이나 계층 구조에 의존하지 않고 UI를 찾을 수 있게 해준다.
/// </summary>
public static class UIRegistry
{
    private static readonly Dictionary<string, RectTransform> _registry =
        new Dictionary<string, RectTransform>();

    /// <summary>
    /// UI를 레지스트리에 등록한다.
    /// </summary>
    public static void Register(string id, RectTransform rect)
    {
        if (string.IsNullOrEmpty(id) || rect == null)
        {
            return;
        }
        _registry[id] = rect;
    }

    /// <summary>
    /// UI를 레지스트리에서 제거한다. (오브젝트 파괴 시 호출)
    /// </summary>
    public static void Unregister(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return;
        }
        _registry.Remove(id);
    }

    /// <summary>
    /// 등록된 UI의 RectTransform을 반환한다. 없으면 null 반환.
    /// </summary>
    public static RectTransform Get(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }
        _registry.TryGetValue(id, out var rect);
        return rect;
    }
}
