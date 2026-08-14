using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemGuard : MonoBehaviour
{
    private void Awake()
    {
        var all = FindObjectsOfType<EventSystem>();
        if (all.Length <= 1) return;

        // Keep the oldest one (DontDestroyOnLoad), destroy the rest
        for (int i = 1; i < all.Length; i++)
            Destroy(all[i].gameObject);
    }
}
