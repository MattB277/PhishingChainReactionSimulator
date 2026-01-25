using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FeedPostInteraction : MonoBehaviour
{
    [Header("Data & State")]
    public TimelinePost postData; // Holds the data for this post

    [Header("UI Components")]
    public TextMeshProUGUI bodyText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI handleText;
    public Image avatarImage;

    [Header("Intervention Buttons")]
    public Button btn_Report; // Stage 1 and 3 interaction
    public Button btn_Comment; // Stage 2 intercation

    public void SetupPost(TimelinePost data, int currentStage)
    {
        this.postData = data;

        // 1. Populate Visuals
        bodyText.text = data.postBody;
        nameText.text = data.displayName;
        handleText.text = "@" + data.handle;
        if (data.avatar != null) avatarImage.sprite = data.avatar;

        // 2. Configure Stage Logic
        ConfigureIntervention(currentStage);
    }

    private void ConfigureIntervention(int stage)
    {
        // Reset specific buttons
        btn_Report.gameObject.SetActive(false);
        btn_Comment.gameObject.SetActive(false);

        // Stage 1: Report Intervention
        if (stage == 1)
        {
            btn_Report.gameObject.SetActive(true);
            
            // Clean old listeners and add the new specific one
            btn_Report.onClick.RemoveAllListeners();
            btn_Report.onClick.AddListener(OnReportClicked);
        }
        // Stage 2: Comment Intervention (Placeholder)
        else if (stage == 2)
        {
            btn_Comment.gameObject.SetActive(true);
        }
    }

    private void OnReportClicked()
    {
        // Send THIS post's data to the central manager
        ReportManager.Instance.OpenReportModal(this);
    }
}