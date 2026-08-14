using UnityEngine;
using TMPro;

public class MissionSystem : MonoBehaviour
{
    public static MissionSystem Instance { get; private set; }

    public int missionStep = 0;

    private TMP_Text missionText;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        missionText = GameObject.Find("MissionText")?.GetComponent<TMP_Text>();

        var save = SaveSystem.Load();
        if (save != null)
            missionStep = save.missionStep;

        UpdateUI();
    }

    public void AdvanceTo(int step)
    {
        if (step <= missionStep) return;
        missionStep = step;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (missionText == null)
            missionText = GameObject.Find("MissionText")?.GetComponent<TMP_Text>();

        if (missionText == null) return;

        switch (missionStep)
        {
            case 0:
                missionText.text = "Mission 1 : Parle au marin du port";
                break;
            case 1:
                missionText.text = "Mission 1 : Trouve Henry au marché";
                break;
            case 2:
                missionText.text = "Mission 1 accomplie !";
                break;
            default:
                missionText.text = "";
                break;
        }
    }

    public void ResetMission()
    {
        missionStep = 0;
        UpdateUI();
    }
}
