using UnityEngine;
using TMPro;

public class PokeWorldController : MonoBehaviour
{
    public GameObject panel;          // PokeWorldPanel
    public Transform content;         // ScrollView/Viewport/Content
    public GameObject wordRowPrefab;  // WordRow prefab

    private WordSystem wordSystem;
    private bool isOpen = false;
    public bool IsOpen() => isOpen;

    private void Start()
    {
        wordSystem = FindObjectOfType<WordSystem>();
        if (panel != null) panel.SetActive(false);
    }

    private void Update()
    {
        var dialogue = FindObjectOfType<DialogueSystem>();

        if (dialogue != null && dialogue.IsOpen())
            return;

        if (Input.GetKeyDown(KeyCode.P) || MobileInput.PokeWorldPressed)
            Toggle();

        if (isOpen && (Input.GetKeyDown(KeyCode.Escape) || MobileInput.ClosePressed))
            Close();
    }

    private void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    private void Open()
    {
        if (panel == null) return;
        isOpen = true;
        panel.SetActive(true);
        RefreshList();
    }

    private void Close()
    {
        if (panel == null) return;
        isOpen = false;
        panel.SetActive(false);
    }

    private void RefreshList()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        if (wordSystem == null)
            wordSystem = FindObjectOfType<WordSystem>();

        if (wordSystem == null) return;

        foreach (var w in wordSystem.capturedWords)
        {
            var row = Instantiate(wordRowPrefab, content);

            var rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(0, 0);
            rt.offsetMax = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(0, 44);

            var tmp = row.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.fontSize = 24;
                tmp.fontStyle = TMPro.FontStyles.Bold;
                tmp.enableWordWrapping = false;
                tmp.overflowMode = TMPro.TextOverflowModes.Overflow;
                tmp.alignment = TMPro.TextAlignmentOptions.Left;
                tmp.margin = new Vector4(20, 0, 20, 0);

                tmp.color = new Color(0.94f, 0.902f, 0.827f, 1f);

                // Color fijo por idioma en toda la app — francés dorado,
                // español azul (ver SpeechCaptionUI.FrenchColor/SpanishColor,
                // mismos valores hexadecimales para que coincida en todas partes).
                tmp.text = $"<color=#FFD166>{w.french}</color>" +
                           $"<color=#5A7A9A>  ——  </color>" +
                           $"<color=#B3D9FF>{w.spanish}</color>";
            }

        }

        if (wordSystem.capturedWords.Count == 0)
        {
            var row = Instantiate(wordRowPrefab, content);
            var rt = row.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot     = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 60);

            var tmp = row.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.fontSize = 18;
                tmp.alignment = TMPro.TextAlignmentOptions.Center;
                tmp.margin = new Vector4(20, 8, 20, 8);
                tmp.text = "<color=#5A7A9A><i>Aucun mot capturé pour l'instant.\nParle avec les habitants de Marseille !</i></color>";
            }
        }
    }
}
