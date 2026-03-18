using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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

    private const float RequiredSuccess = 0.60f;   // 80%
    private const float MaxSuspicion = 0.60f;      // 40%

    private struct SubmissionResult
    {
        public bool IsWin;
        public float Success;      // 0-1
        public float Suspicion;    // 0-1
        public List<string> Messages;
    }
    
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

        if (currentModules == null || currentModules.Count == 0)
        {
            Debug.Log("PhishingComposer: Cannot send empty messages");
            return;
        }

        float success = Mathf.Clamp01(currentModules.Sum(m => m.successModifier));
        float suspicion = Mathf.Clamp01(currentModules.Sum(m => m.suspicionModifier));

        var result = EvaluateSubmission(currentModules, success, suspicion);

        ShowResult(result);
    }

    private SubmissionResult EvaluateSubmission(List<PhishingModule> modules, float success, float suspicion)
    {
        var messages = new List<string>();

        // Structural validity checks
        bool hasPayload = modules.Any(m => m.type == ModuleType.PayloadLink);
        bool hasCta = modules.Any(m => m.type == ModuleType.CallToAction);
        bool hasHook = modules.Any(m => m.type == ModuleType.Hook);

        if (!hasPayload)
            messages.Add("No payload/link selected. Without a link or attachment, the victim has nothing to click - the phish can't do anything.");

        if (!hasCta)
            messages.Add("No call-to-action selected. You haven't asked the victim to do anything, so even a convincing post won't convert.");

        if (!hasHook)
            messages.Add("No hook selected. The message lacks a reason for the victim to engage (offer, news, claim, etc.).");

        // If the basics are missing, fail immediately with explanatory feedback.
        bool structurallyValid = hasPayload && hasCta;
        if (!structurallyValid)
        {
            return new SubmissionResult
            {
                IsWin = false,
                Success = success,
                Suspicion = suspicion,
                Messages = messages
            };
        }

        // Stat gates
        if (success < RequiredSuccess)
            messages.Add($"Success is too low: {(success * 100f):0}% (need at least {(RequiredSuccess * 100f):0}%). Add a stronger hook, authority cue, or social proof.");

        if (suspicion >= MaxSuspicion)
            messages.Add($"Suspicion is too high: {(suspicion * 100f):0}% (must be under {(MaxSuspicion * 100f):0}%). Remove obvious pressure or a dodgy payload and try a cleaner call to action.");

        bool isWin = success >= RequiredSuccess && suspicion < MaxSuspicion;

        if (isWin)
            messages.Add($"Success: {(success * 100f):0}% and Suspicion: {(suspicion * 100f):0}%. This is believable enough to catch someone out.");

        return new SubmissionResult
        {
            IsWin = isWin,
            Success = success,
            Suspicion = suspicion,
            Messages = messages
        };
    }

    private void ShowResult(SubmissionResult result)
    {
        resultPanel.SetActive(true);

        // Multi line feedback
        resultText.text = string.Join("\n\n", result.Messages);

        nextLevelButton.gameObject.SetActive(result.IsWin);
        closeResultButton.gameObject.SetActive(!result.IsWin);
    }

    public void AdvanceLevel()
    {
        resultPanel.SetActive(false);
        ClearComposer();
        int nextLevelIdx = ScenarioManager.Instance.CurrentLevelIdx + 2; // Why is this +2? CurrentLevelIdx is being decremented somewhere
        ScenarioManager.Instance.LoadScenario(nextLevelIdx);
        palette.PopulatePallete(nextLevelIdx);
        Debug.Log($"Finished {ScenarioManager.Instance.CurrentLevelIdx}. Advanced to level index {nextLevelIdx}");
    }

    public void RetryLevel()
    {
        resultPanel.SetActive(false);
        ClearComposer();
        Debug.Log($"Retry failed level");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
