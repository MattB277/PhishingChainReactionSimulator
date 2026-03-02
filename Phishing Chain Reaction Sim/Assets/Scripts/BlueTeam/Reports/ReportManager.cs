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
        // TODO: Add completion condition (e.g. all phish posts reported)
        // For now, each submission counts as stage complete
        //if (BlueTeamManager.Instance != null)
        //    BlueTeamManager.Instance.NotifyStageComplete();
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