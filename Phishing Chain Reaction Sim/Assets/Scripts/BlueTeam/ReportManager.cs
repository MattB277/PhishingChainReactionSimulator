using UnityEngine;
using UnityEngine.UI;

public class ReportManager : MonoBehaviour
{
    public static ReportManager Instance;

    [Header("UI References")]
    public GameObject modalPanel;
    public Toggle[] reasonToggles;
    public Button submitButton; // Held inside the modal window

    private FeedPostInteraction currentTarget; // Post which has been reported

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        modalPanel.SetActive(false);
    }

    public void OpenReportModal(FeedPostInteraction post)
    {
        currentTarget = post;
        modalPanel.SetActive(true);

        // Reset the toggles
        foreach (var toggle in reasonToggles)
        {
            toggle.isOn = false;
        }
    }

    public void SubmitReport()
    {
        if (currentTarget == null) return;

        // Find which toggle is active
        PhishReason selectedReason = PhishReason.Safe;

        for (int i = 0; i < reasonToggles.Length; i++)
        {
            if (reasonToggles[i].isOn)
            {
                // Map Index 0 to Enum 1, Index 1 to Enum 2, etc.
                // PhishReason Enums are in same order as toggles top -> bottom
                selectedReason = (PhishReason)(i + 1); 
                break;
            }
        }
        // Check answer
        EvaluateDecision(selectedReason);
        
        // Close modal panel
        modalPanel.SetActive(false);
    }

    private void EvaluateDecision(PhishReason userReason)
    {
        bool isPhish = currentTarget.postData.isPhish;
        PhishReason correctReason = currentTarget.postData.correctReason;

        // User correctly identified a phish and the correct reason?
        if (isPhish && userReason == correctReason)
        {
            Debug.Log("<color=green>SUCCESS: Phish caught correctly!</color>");
            // TODO: Add score / show success popup
        }
        else if (!isPhish)
        {
            Debug.Log("<color=red>FAIL: You reported a safe post!</color>");
        }
        else
        {
             Debug.Log($"<color=orange>CLOSE: It was a phish, but reason was wrong. (Expected: {correctReason})</color>");
        }
    }
    
    // Called from the "X" button on the modal
    public void CancelReport()
    {
        modalPanel.SetActive(false);
    }
}