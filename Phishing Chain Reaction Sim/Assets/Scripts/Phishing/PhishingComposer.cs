using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PhishingComposer : MonoBehaviour
{
    [Header("Module References")]
    [SerializeField] private MessageDropZone dropZone;
    [SerializeField] private ModulePalette palette;
    
    [Header("Stat UI References")]
    [SerializeField] private Slider successSlider;
    [SerializeField] private Slider suspicionSlider;

    [Header("Result UI")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private Button nextLevelButton; // only appears on win
    [SerializeField] private Button closeResultButton; // only appears on fail
    
    void Start()
    {
        // hide result panel
        if(resultPanel) resultPanel.SetActive(false);
    }
    public void OnModulesChanged()
    {
        var currentModules = dropZone.GetCurrentModuleData();

        float successChance = Mathf.Clamp01(currentModules.Sum(m=> m.successModifier));
        float suspicionLevel = Mathf.Clamp01(currentModules.Sum(m=> m.suspicionModifier));

        successSlider.value = successChance;
        suspicionSlider.value = suspicionLevel;
    }

    public void ClearComposer()
    {
        dropZone.ClearAllModules();
        OnModulesChanged(); // recalculate sliders
    }
    
    public void SendPost()
    {
        var currentModules = dropZone.GetCurrentModuleData();

        if (currentModules.Count == 0)
        {
            Debug.Log("PhishingComposer: Cannot send empty messages");
            return;
        }

        // calculate final score
        float finalScorePercent = Mathf.Clamp01(successSlider.value - suspicionSlider.value) * 100f;
        // get threshold from ScenarioManager
        float scenarioThreshold = ScenarioManager.Instance.GetCurrentTreshold();

        bool isWin = finalScorePercent >= scenarioThreshold;

        ShowResult(isWin, finalScorePercent, scenarioThreshold);
    }

    private void ShowResult(bool isWin, float score, float threshold)
    {
        resultPanel.SetActive(true);

        if (isWin)
        {
            resultText.text = "Placeholder Success";
            nextLevelButton.gameObject.SetActive(true);
            closeResultButton.gameObject.SetActive(false);
        }
        else
        {
            resultText.text = "Placeholder Fail";
            nextLevelButton.gameObject.SetActive(false);
            closeResultButton.gameObject.SetActive(true);
        }
    }

    private void AdvanceLevel()
    {
        resultPanel.SetActive(false);
        ClearComposer();
        int nextLevelIdx = ScenarioManager.Instance.CurrentLevelIdx + 1;
        ScenarioManager.Instance.LoadScenario(nextLevelIdx);
        palette.PopulatePallete(nextLevelIdx);
        Debug.Log($"Finished {ScenarioManager.Instance.CurrentLevelIdx}. Advanced to level index {nextLevelIdx}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
