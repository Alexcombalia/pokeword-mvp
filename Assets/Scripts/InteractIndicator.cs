using UnityEngine;

public class InteractIndicator : MonoBehaviour
{
    [Tooltip("Child GameObject with the visual prompt (sprite or text showing E)")]
    [SerializeField] private GameObject indicator;

    private void Start()
    {
        Hide();
    }

    public void Show() { if (indicator != null) indicator.SetActive(true); }
    public void Hide() { if (indicator != null) indicator.SetActive(false); }
}
