using UnityEngine;

public class WaterFountain : MonoBehaviour
{
    [SerializeField] private float waterRestore = 0.3f;
    private ResourceSystem resources;
    private bool playerNear = false;

    private void Start()
    {
        resources = FindObjectOfType<ResourceSystem>();
    }

    private void Update()
    {
        if (!playerNear) return;

        var dialogue = FindObjectOfType<DialogueSystem>();
        if (dialogue != null && dialogue.IsOpen())
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (resources != null)
            {
                resources.Recover(0f, waterRestore, 0f);
                Debug.Log("Bebes agua de la fuente.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNear = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNear = false;
    }
}