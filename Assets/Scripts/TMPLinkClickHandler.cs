using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TMPLinkClickHandler : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text text;

    public System.Action<string> OnLinkClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("CLICK detectado en texto");

        if (text == null) return;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, eventData.pressEventCamera);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
            string linkId = linkInfo.GetLinkID();
            Debug.Log("Link detectado: " + linkId);
            OnLinkClicked?.Invoke(linkId);
        }
    }

}
