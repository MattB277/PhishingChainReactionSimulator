using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnalysisManager : MonoBehaviour
{
    public static AnalysisManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject modalPanel;
    public TextMeshProUGUI targetPostText;
    public TextMeshProUGUI activeEventText;

    [Header("Stage 3 Inputs")]
    public Toggle[] campaignToggles; // Indexes map to PhishCampaignType enum (skip None)
    public Toggle[] impactToggles;   // Indexes map to PhishImpactType enum (skip None)

    private TimelinePost currentTarget;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (modalPanel != null) modalPanel.SetActive(false);
    }

    public void OpenAnalysisModal(TimelinePost post)
    {
        currentTarget = post;

        if (targetPostText != null)
            targetPostText.text = post.postData.postBody;

        if (activeEventText != null)
        {
            string eventLabel = string.IsNullOrWhiteSpace(post.postData.eventTag) ? "No active event" : post.postData.eventTag;
            activeEventText.text = $"Active trend: {eventLabel}";
        }

        ResetToggles(campaignToggles);
        ResetToggles(impactToggles);

        if (modalPanel != null) modalPanel.SetActive(true);
    }

    public void SubmitAnalysis()
    {
        if (currentTarget == null) return;

        PhishCampaignType selectedCampaign = GetSelectedCampaign();
        PhishImpactType selectedImpact = GetSelectedImpact();

        bool campaignChosen = selectedCampaign != PhishCampaignType.None;
        bool impactChosen = selectedImpact != PhishImpactType.None;

        Debug.Log($"User submitted analysis: Campaign={selectedCampaign}, Impact={selectedImpact}");
        Debug.Log($"Post data: IsPhish={currentTarget.postData.isPhish}, CampaignType={currentTarget.postData.campaignType}, ImpactType={currentTarget.postData.impactType}");

        // Reject partial and empty submissions
        if (campaignChosen && impactChosen)
        {
            EvaluateDecision(selectedCampaign, selectedImpact);

            // Remove the post from the feed
            if (BlueTeamManager.Instance != null)
                BlueTeamManager.Instance.timelineManager.RemovePost(currentTarget);

            if (modalPanel != null) modalPanel.SetActive(false);

            // Notify central manager that this post has been handled
            if (BlueTeamManager.Instance != null)
                BlueTeamManager.Instance.NotifyPostHandled();
        } else {
            LogFailure("Choose both a campaign context and an impact type before submitting.");
            return;
        }
    }

    public void CancelAnalysis()
    {
        if (modalPanel != null) modalPanel.SetActive(false);
    }

    private PhishCampaignType GetSelectedCampaign()
    {
        for (int i = 0; i < campaignToggles.Length; i++)
        {
            if (campaignToggles[i] != null && campaignToggles[i].isOn)
            {
                return (PhishCampaignType)(i + 1);
            }
        }

        return PhishCampaignType.None;
    }

    private PhishImpactType GetSelectedImpact()
    {
        for (int i = 0; i < impactToggles.Length; i++)
        {
            if (impactToggles[i] != null && impactToggles[i].isOn)
            {
                return (PhishImpactType)(i + 1);
            }
        }

        return PhishImpactType.None;
    }

    private void EvaluateDecision(PhishCampaignType userCampaign, PhishImpactType userImpact)
    {
        TimelinePostData post = currentTarget.postData;

        if (!post.isPhish)
        {
            LogFailure("False positive: this post is legitimate.");
            return;
        }

        bool campaignCorrect = userCampaign == post.campaignType;
        bool impactCorrect = userImpact == post.impactType;

        if (campaignCorrect && impactCorrect)
        {
            LogSuccess($"Excellent analysis. You identified {post.campaignType} with likely impact {post.impactType}.");
            return;
        }

        if (campaignCorrect)
        {
            LogFailure($"Good context read, but impact is off. Expected {post.impactType}.");
            return;
        }

        if (impactCorrect)
        {
            LogFailure($"Good impact read, but campaign context is off. Expected {post.campaignType}.");
            return;
        }

        LogFailure($"Missed both dimensions. Expected context {post.campaignType}, impact {post.impactType}.");
    }

    private void ResetToggles(Toggle[] toggles)
    {
        if (toggles == null) return;

        foreach (Toggle toggle in toggles)
        {
            if (toggle != null) toggle.isOn = false;
        }
    }

    private void LogSuccess(string message)
    {
        if (FeedbackManager.Instance != null)
        {
            FeedbackManager.Instance.ShowSuccess(message);
            return;
        }

        Debug.Log($"<color=green>SUCCESS: {message}</color>");
    }

    private void LogFailure(string message)
    {
        if (FeedbackManager.Instance != null)
        {
            FeedbackManager.Instance.ShowFailure(message);
            return;
        }

        Debug.Log($"<color=red>FAILURE: {message}</color>");
    }
}
