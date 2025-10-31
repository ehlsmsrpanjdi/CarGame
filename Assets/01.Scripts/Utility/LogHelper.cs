using UnityEngine;

public static class LogHelper
{
    public static void LogError(string message, Object _context)
    {
#if UNITY_EDITOR
        Debug.LogError($"{message} {_context}");
#endif
    }
    public static void LogError(string message)
    {
#if UNITY_EDITOR
        Debug.LogError($"{message}");
#endif
    }

    public static void LogWarrning(string message, Object _context)
    {
#if UNITY_EDITOR
        Debug.LogWarning($"{message} {_context}");
#endif
    }

    public static void LogWarrning(string message)
    {
#if UNITY_EDITOR
        Debug.LogWarning($"{message}");
#endif
    }

    public static void Log(string message, Object _context)
    {
#if UNITY_EDITOR
        Debug.Log($"{message} {_context}");
#endif
    }

    public static void Log(string message)
    {
#if UNITY_EDITOR
        Debug.Log(message);
#endif
    }
}
