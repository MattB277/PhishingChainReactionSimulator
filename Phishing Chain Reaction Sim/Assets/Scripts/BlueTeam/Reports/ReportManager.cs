using UnityEngine;
using UnityEngine.UI;

public class ReportManager : MonoBehaviour
{
    public static ReportManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject modalPanel;
    public Toggle[] reasonToggles;
    public Button submitButton; // Held inside the modal window

    private TimelinePost currentTarget; // Post which has been reported

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        modalPanel.SetActive(false);
    }

    public void OpenReportModal(TimelinePost post)
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
        // Return early if no reason chosen
        if (selectedReason == PhishReason.Safe) return;
        
        // Check answer
        EvaluateDecision(selectedReason);
        
        // Remove the post from the feed
        if (BlueTeamManager.Instance != null)
            BlueTeamManager.Instance.timelineManager.RemovePost(currentTarget);

        // Close modal panel
        modalPanel.SetActive(false);

        // Notify central manager that this post has been handled
        if (BlueTeamManager.Instance != null)
            BlueTeamManager.Instance.NotifyPostHandled();
    }

    private void EvaluateDecision(PhishReason userReason)
    {
        bool isPhish = currentTarget.postData.isPhish;
        PhishReason correctReason = currentTarget.postData.correctReason;

        // User correctly identified a phish and the correct reason
        if (isPhish && userReason == correctReason)
        {
            FeedbackManager.Instance.ShowSuccess($"Correct! You identified the {correctReason} tactic.");
        }
        else if (!isPhish)
        {
            FeedbackManager.Instance.ShowFailure("False alarm! This post is actually safe, though it is good practice to be cautious.");
        }
        else
        {
            FeedbackManager.Instance.ShowFailure($"It was a phish, but the tactic used was {correctReason}, not {userReason}.");
        }
    }
    
    // Called from the "X" button on the modal
    public void CancelReport()
    {
        modalPanel.SetActive(false);
    }
}