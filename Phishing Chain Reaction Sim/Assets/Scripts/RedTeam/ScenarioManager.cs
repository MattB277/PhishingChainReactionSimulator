using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public struct ScenarioProfile
{
    [Header("UI Display")]
    [TextArea(3, 5)]
    public string scenarioBrief;

    [Header("Win Condition")]
    [Range(0,100)]
    public float winThreshold;
}

public class ScenarioManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scenarioText;
    [SerializeField] private StageContextPanel stageContextPanel;

    private const string RedTeamContext = "Red Team Briefing: Craft a convincing phishing message by combining one card of each module type. Balance realism and persuasion to maximise the attack score.";
    private bool hasShownInitialContext;

    [Header("Scenario Configuration")]
    [SerializeField] private List<ScenarioProfile> scenarios;

    public int CurrentLevelIdx {get; private set; } = 0;

    // Singleton instance for easy access
    public static ScenarioManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else
        {
            Destroy(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadScenario(1);
        ShowInitialContextOnce();
    }

    public void LoadScenario(int level)
    {
        CurrentLevelIdx = level - 1;

        if (CurrentLevelIdx >= 0 && CurrentLevelIdx < scenarios.Count)
        {
            if (scenarioText != null)
                scenarioText.text = scenarios[CurrentLevelIdx].scenarioBrief; // set Text box label to ScenarioBrief string
        } else
        {
            Debug.LogWarning($"ScenarioManager: No text found for level {level}");
        }
    }

    private void ShowInitialContextOnce()
    {
        if (hasShownInitialContext) return;
        hasShownInitialContext = true;

        if (stageContextPanel != null)
            stageContextPanel.Show(1, RedTeamContext);
        else
            Debug.LogWarning("ScenarioManager: StageContextPanel reference missing for Red Team context popup.");
    }

    public float GetCurrentTreshold()
    {
        if (CurrentLevelIdx >= 0 && CurrentLevelIdx < scenarios.Count)
        {
            return scenarios[CurrentLevelIdx].winThreshold;
        }
        Debug.LogWarning($"ScenarioManager: Fallback for winThreshold on level {CurrentLevelIdx}");
        return 50f; // fallback to 50%
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
