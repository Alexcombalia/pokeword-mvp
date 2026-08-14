using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public static Vector2 Direction { get; private set; }

    public RectTransform knob;

    private RectTransform bgRect;
    private Canvas rootCanvas;
    private float radius;

    private void Awake()
    {
        bgRect = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
        radius = bgRect.sizeDelta.x * 0.4f;
    }

    public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

    public void OnDrag(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                bgRect, eventData.position, rootCanvas.worldCamera, out Vector2 local))
            return;

        Vector2 clamped      = Vector2.ClampMagnitude(local, radius);
        knob.anchoredPosition = clamped;
        Direction            = clamped / radius;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Direction             = Vector2.zero;
        knob.anchoredPosition = Vector2.zero;
    }
}
