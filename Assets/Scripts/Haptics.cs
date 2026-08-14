using UnityEngine;

public static class Haptics
{
    // Short tap — for entering NPC range, scene exits
    public static void Light() => Vibrate(40L);

    // Stronger pulse — for capturing a word
    public static void Medium() => Vibrate(80L);

    private static void Vibrate(long milliseconds)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using var player   = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
            vibrator.Call("vibrate", milliseconds);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[Haptics] Vibrate failed: {e.Message}");
        }
#endif
    }
}
