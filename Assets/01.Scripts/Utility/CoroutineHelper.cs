using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class FloatComparer : IEqualityComparer<float>
{
    bool IEqualityComparer<float>.Equals(float x, float y)
    {
        return x == y;
    }
    int IEqualityComparer<float>.GetHashCode(float obj)
    {
        return obj.GetHashCode();
    }
}

public static class CoroutineHelper
{
    private static readonly Dictionary<float, WaitForSeconds> WaitForSecondsCache = new Dictionary<float, WaitForSeconds>(new FloatComparer());

    public static WaitForSeconds WaitTime(float _second)
    {
        if (true == WaitForSecondsCache.TryGetValue(_second, out WaitForSeconds waitForSeconds))
        {
            return waitForSeconds;
        }
        else
        {
            WaitForSecondsCache.Add(_second, new WaitForSeconds(_second));
        }
        return WaitForSecondsCache[_second];
    }
}
