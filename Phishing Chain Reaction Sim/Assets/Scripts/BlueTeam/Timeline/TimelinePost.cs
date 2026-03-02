using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimelinePost : MonoBehaviour
{
    [Header("Data & State")]
    public TimelinePostData postData;
    private int currentStage;

    [Header("UI Components")]
    public TextMeshProUGUI bodyText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI handleText;
    public Image avatarImage;

    [Header("Intervention Buttons")]
    public Button btn_Report; // Stage 1 and 3 interaction
    public Button btn_Comment; // Stage 2 intercation

    public void SetupPost(TimelinePostData data, int stage)
    {
        this.postData = data;
        this.currentStage = stage;

        // 1. Populate Visuals
        bodyText.text = data.postBody;
        nameText.text = data.displayName;
        handleText.text = "@" + data.handle;
        if (data.avatar != null) avatarImage.sprite = data.avatar;

        // 2. Configure Stage Logic
        ConfigureIntervention();
    }

    private void ConfigureIntervention()
    {
        // This could absolutely be made shorter with the OnReport/OnComment functions 
        // Being mapped to a variabe
        // Reset specific buttons
        btn_Report.gameObject.SetActive(false);
        btn_Comment.gameObject.SetActive(false);

        // Stage 1: Report Intervention OR Stage 3: Contextual Analysis Intervention
        if (currentStage == 1 || currentStage == 3)
        {
            btn_Report.gameObject.SetActive(true);
            
            // Clean old listeners and add the new specific one
            btn_Report.onClick.RemoveAllListeners();
            btn_Report.onClick.AddListener(OnReportClicked);
        }
        // Stage 2: Comment Intervention
        else if (currentStage == 2)
        {
            btn_Comment.gameObject.SetActive(true);

            btn_Comment.onClick.RemoveAllListeners();
            btn_Comment.onClick.AddListener(OnCommentClicked);
        }
    }

    private void OnCommentClicked()
    {
        Debug.Log($"Comment button clicked for post: {postData.id}");

        if (CommentManager.Instance != null)
        {
            CommentManager.Instance.OpenCommentModal(this);
        }
        else
        {
            Debug.LogError("CommentManager is missing in the scene.");
        }
    }

    private void OnReportClicked()
    {
        if (currentStage == 3)
        {
            if (AnalysisManager.Instance != null)
            {
                AnalysisManager.Instance.OpenAnalysisModal(this);
            }
            else
            {
                Debug.LogError("AnalysisManager is missing in the scene.");
            }

            return;
        }

        // Stage 1 default report flow
        if (ReportManager.Instance != null)
        {
            ReportManager.Instance.OpenReportModal(this);
        }
        else
        {
            Debug.LogError("ReportManager is missing in the scene.");
        }
    }
}