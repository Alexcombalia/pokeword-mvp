using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroVideoController : MonoBehaviour
{
    [SerializeField] private float waitSeconds = 3f;

    private void Start()
    {
        Invoke(nameof(GoToPuerto), waitSeconds);
    }

    private void GoToPuerto()
    {
        SceneManager.LoadScene("Puerto");
    }
}
