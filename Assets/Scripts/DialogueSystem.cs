using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    [Header("Link Click Handler (on DialogueText)")]
    public TMPLinkClickHandler linkClickHandler;

    private WordSystem wordSystem;

    // Diccionario de capturas: linkId -> (french, spanish)
    private Dictionary<string, (string fr, string es)> currentCapturable = new Dictionary<string, (string fr, string es)>();

    public GameObject dialoguePanel;     // DialoguePanel
    public TMP_Text dialogueText;        // DialogueText

    private Queue<string> lines = new Queue<string>();
    private bool isOpen = false;

    private ResourceSystem resources;
    [Header("Chat UI (optional)")]
    public TMP_InputField dialogueInput;
    public Button sendButton;
    private BakerChatClient bakerClient;
    private bool chatMode = false;
    public Button buyBreadButton;
    private BakerShop currentBakerShop;

    private void Awake()
    {
        resources = FindObjectOfType<ResourceSystem>();
        wordSystem = FindObjectOfType<WordSystem>();

        if (linkClickHandler != null)
        {
            linkClickHandler.OnLinkClicked += HandleLinkClicked;
        }
        bakerClient = FindObjectOfType<BakerChatClient>();
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(SendChatMessage);
        }
    }


    private void Update()
    {
        if (!isOpen) return;

        if (chatMode && Input.GetKeyDown(KeyCode.Return))
        {
            SendChatMessage();
        }

        if (!chatMode && Input.GetKeyDown(KeyCode.Space))
        {
            NextLine();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void OpenBakerChat()
    {
        gameObject.SetActive(true);
        if (dialoguePanel != null && dialoguePanel.transform.root != null)
            dialoguePanel.transform.root.gameObject.SetActive(true);

        dialoguePanel.SetActive(true);
        isOpen = true;
        chatMode = true;

        if (resources != null) resources.DrainOnConversation();

        if (dialogueText != null)
            dialogueText.text = "Bonjour. Je suis le boulanger.\nAppuie sur B pour acheter du pain.";
        if (dialogueInput != null)
        {
            dialogueInput.gameObject.SetActive(true);
            dialogueInput.text = "";
            dialogueInput.ActivateInputField();
        }

        if (sendButton != null)
            sendButton.gameObject.SetActive(true);

   

        currentCapturable.Clear();

        if (bakerClient != null)
        {
            StartCoroutine(bakerClient.ResetMemory(
                onDone: () =>
                {
                    Debug.Log("Mémoire du boulanger réinitialisée.");
                },
                onError: (err) =>
                {
                    Debug.LogWarning("Erreur reset mémoire boulanger: " + err);
                }
            ));
        }

    }

    private void SendChatMessage()
    {
        if (!chatMode) return;
        if (dialogueInput == null) return;

        string msg = dialogueInput.text.Trim();
        bool autoBuyBread = false;
        bool breadBought = false;

        if (currentBakerShop != null && BakerIntentParser.WantsToBuyBread(msg))
        {
            autoBuyBread = true;
            breadBought = currentBakerShop.BuyBread();
        }

        StartCoroutine(bakerClient.SendMessage(
            msg,
            onReply: (reply) =>
            {
                var processedReply = DynamicWordHighlighter.ProcessText(reply, out var generatedCapturable);

                currentCapturable.Clear();
                foreach (var kvp in generatedCapturable)
                {
                    currentCapturable[kvp.Key] = kvp.Value;
                }

                string extra = "";

                if (autoBuyBread)
                {
                    if (breadBought)
                        extra = "\n\n[Boutique] Tu achètes du pain pour 5$.";
                    else
                        extra = "\n\n[Boutique] Tu n'as pas assez d'argent.";
                }

                dialogueText.text = "Toi: " + msg + "\n\n" + "Boulanger: " + processedReply + extra;
            },
            onError: (err) =>
            {
                dialogueText.text = "Erreur serveur: " + err;
            }
        ));
    }

    public bool IsOpen() => isOpen;

    public void Open(List<string> dialogueLines)
    {
        if (dialogueLines == null || dialogueLines.Count == 0) return;

        lines.Clear();
        foreach (var l in dialogueLines) lines.Enqueue(l);

        dialoguePanel.SetActive(true);
        isOpen = true;

        // drenaje por conversación (una vez al abrir)
        if (resources != null) resources.DrainOnConversation();

        NextLine();
    }

    private void NextLine()
    {
        if (lines.Count == 0)
        {
            Close();
            return;
        }

        dialogueText.text = lines.Dequeue();
    }
    private void HandleLinkClicked(string linkId)
    {
        if (!isOpen) return;
        if (wordSystem == null) return;

        string normalized = linkId.ToLower();

        if (currentCapturable.TryGetValue(normalized, out var pair))
        {
            wordSystem.Capture(pair.fr, pair.es);
        }
    }


    public void SetCapturableWords(Dictionary<string, (string fr, string es)> capturable)
    {
        currentCapturable = capturable ?? new Dictionary<string, (string fr, string es)>();
    }
    public void Close()
    {
        currentCapturable.Clear();
        chatMode = false;

        if (dialogueInput != null) dialogueInput.gameObject.SetActive(false);
        if (sendButton != null) sendButton.gameObject.SetActive(false);

        dialoguePanel.SetActive(false);
        isOpen = false;

        if (buyBreadButton != null)
            buyBreadButton.gameObject.SetActive(false);
    }

    public void OpenBakerChat(BakerShop bakerShop)
    {
        currentBakerShop = bakerShop;

        gameObject.SetActive(true);
        if (dialoguePanel != null && dialoguePanel.transform.root != null)
            dialoguePanel.transform.root.gameObject.SetActive(true);

        dialoguePanel.SetActive(true);
        isOpen = true;
        chatMode = true;

        if (resources != null) resources.DrainOnConversation();

        if (dialogueText != null)
            dialogueText.text = "Bonjour. Je suis le boulanger.\nVeux-tu du pain pour 5$ ?";

        if (dialogueInput != null)
        {
            dialogueInput.gameObject.SetActive(true);
            dialogueInput.text = "";
            dialogueInput.ActivateInputField();
        }

        if (sendButton != null)
            sendButton.gameObject.SetActive(true);

        if (buyBreadButton != null)
        {
            buyBreadButton.gameObject.SetActive(true);
            buyBreadButton.onClick.RemoveAllListeners();
            buyBreadButton.onClick.AddListener(() =>
            {
                if (currentBakerShop != null)
                {
                    currentBakerShop.BuyBread();
                }
            });
        }

        currentCapturable.Clear();

        if (bakerClient != null)
        {
            StartCoroutine(bakerClient.ResetMemory(
                onDone: () =>
                {
                    Debug.Log("Mémoire du boulanger réinitialisée.");
                },
                onError: (err) =>
                {
                    Debug.LogWarning("Erreur reset mémoire boulanger: " + err);
                }
            ));
        }
    }



}
