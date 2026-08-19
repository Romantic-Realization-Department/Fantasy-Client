using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WaitForSeconds 인스턴스를 float 키로 캐싱하여 GC 부하를 줄이는 유틸리티.
/// 동일한 대기 시간에 대해 항상 같은 객체를 반환한다.
/// </summary>
public static class YieldInstructionCache
{
    private static readonly Dictionary<float, WaitForSeconds> cache = new();

    public static WaitForSeconds WaitForSeconds(float seconds)
    {
        if (!cache.TryGetValue(seconds, out WaitForSeconds wait))
        {
            wait = new WaitForSeconds(seconds);
            cache[seconds] = wait;
        }
        return wait;
    }
}
