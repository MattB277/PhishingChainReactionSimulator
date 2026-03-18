using UnityEngine;
using TMPro;

public class CommentManager : MonoBehaviour
{
    public static CommentManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject modalPanel;
    public TextMeshProUGUI targetPostText;
    [Header("Palette Reference")]
    public CommentCardPalette palette;
    
    [Header("Drop Zones")]
    public CommentDropZone categoryZone;
    public CommentDropZone reasoningZone;
    public CommentDropZone adviceZone;

    private TimelinePost currentTarget;

    void Awake()
    {
        Instance = this;
        modalPanel.SetActive(false);
    }

    public void OpenCommentModal(TimelinePost post)
    {
        currentTarget = post;
        targetPostText.text = post.postData.postBody;
        modalPanel.SetActive(true);
        
        PopulatePalette();
    }

    private void PopulatePalette()
    {
        if (palette != null)
        {
            palette.PopulatePallete();
        } else
        {
            Debug.LogError("CommentCardPalette reference is missing in CommentManager.");
        }
    }

    public void SubmitComment()
    {
        CommentCard catCard = categoryZone.GetCardInZone();
        CommentCard reasonCard = reasoningZone.GetCardInZone();
        CommentCard adviceCard = adviceZone.GetCardInZone();
        // 1. Validation Check: Are all slots filled?
        if (catCard == null || reasonCard == null || adviceCard == null)
        {
            Debug.LogWarning("Please fill all three sections before posting.");
            return;
        }

        // 2. Evaluation Logic
        EvaluateComment(catCard, reasonCard, adviceCard);

        // Remove the post from the feed
        if (BlueTeamManager.Instance != null)
            BlueTeamManager.Instance.timelineManager.RemovePost(currentTarget);

        // Notify central manager that this post has been handled
        if (BlueTeamManager.Instance != null)
            BlueTeamManager.Instance.NotifyPostHandled();

        CloseModal();
    }

    public void ClearCards()
    {
        // Use the safe ClearZone method to ensure lists and visuals stay in sync
        categoryZone.ClearZone();
        reasoningZone.ClearZone();
        adviceZone.ClearZone();
    }

    public void CloseModal()
    {
        ClearCards();
        modalPanel.SetActive(false);
    }

    private void EvaluateComment(CommentCard catCard, CommentCard reasonCard, CommentCard adviceCard)
    {
        TimelinePostData post = currentTarget.postData;

        // --- Safe post reported as phish ---
        if (!post.isPhish)
        {
            FeedbackManager.Instance.ShowFailure("False alarm! This post is actually safe.");
            return;
        }

        // --- Check each dimension ---
        bool categoryCorrect = catCard.Data.linkedImpact   == post.impactType;
        bool reasonCorrect   = reasonCard.Data.linkedReason == post.correctReason;
        bool adviceCorrect   = adviceCard.Data.linkedAdvice == PhishAdviceMapping.FromReason(post.correctReason);

        // --- Feedback ---
        if (reasonCorrect && categoryCorrect && adviceCorrect)
        {
            FeedbackManager.Instance.ShowSuccess(
                $"Excellent comment! You correctly categorised a {post.impactType} phish, spotted the {post.correctReason} giveaway, and gave the right advice.");
        }
        else if (reasonCorrect && adviceCorrect)
        {
            FeedbackManager.Instance.ShowSuccess(
                $"Good eye! You spotted {post.correctReason} and gave solid advice, but the phish category is {post.impactType}.");
        }
        else if (reasonCorrect)
        {
            FeedbackManager.Instance.ShowSuccess(
                $"You identified the {post.correctReason} giveaway, but your category or advice needs work.");
        }
        else
        {
            FeedbackManager.Instance.ShowFailure(
                $"Not quite. The giveaway here is {post.correctReason}, not {reasonCard.Data.linkedReason}.");
        }
    }
}