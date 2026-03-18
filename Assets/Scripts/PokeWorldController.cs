using UnityEngine;
using TMPro;

public class PokeWorldController : MonoBehaviour
{
    public GameObject panel;          // PokeWorldPanel
    public Transform content;         // ScrollView/Viewport/Content
    public GameObject wordRowPrefab;  // WordRow prefab

    private WordSystem wordSystem;
    private bool isOpen = false;

    private void Start()
    {
        wordSystem = FindObjectOfType<WordSystem>();
        if (panel != null) panel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Toggle();
        }

        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
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
        // Limpiar contenido
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        if (wordSystem == null)
            wordSystem = FindObjectOfType<WordSystem>();

        if (wordSystem == null)
            return;

        foreach (var w in wordSystem.capturedWords)
        {
            var row = Instantiate(wordRowPrefab, content);

            // 🔹 Ajustar RectTransform para que ocupe todo el ancho
            var rect = row.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(0, 60); // Altura fija

            // 🔹 Ajustar texto
            var tmp = row.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.enableWordWrapping = false;
                tmp.overflowMode = TMPro.TextOverflowModes.Overflow;
                tmp.alignment = TMPro.TextAlignmentOptions.Left;
                tmp.text = $"{w.french}  —  {w.spanish}";
            }
        }
    }

}
