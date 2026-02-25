using UnityEngine;
using TMPro;
using System;

public class CommentManager : MonoBehaviour
{
    public static CommentManager Instance;

    [Header("UI References")]
    public GameObject modalPanel;
    public TextMeshProUGUI targetPostText;
    
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
        targetPostText.text = post.postBody;
        modalPanel.SetActive(true);
        
        PopulatePalette();
    }

    private void PopulatePalette()
    {
        
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
            // Ideally: Flash the empty zones red or show a UI tooltip here
            return;
        }

        // 2. Evaluation Logic
        // We evaluate primarily based on the Reasoning matching the PhishReason.
        bool isPhish = currentTarget.isPhish;
        PhishReason correctReason = currentTarget.correctReason;

        if (isPhish && reasonCard.Data.linkedReason == correctReason)
        {
            // SUCCESS
            int victimsSaved = 25; // Placeholder for your herd immunity calculation
            
            // Assuming you have a FeedbackManager
            FeedbackManager.Instance.ShowSuccess($"Excellent Comment! You accurately identified the {correctReason} tactic and saved {victimsSaved} users.");
        }
        else if (isPhish)
        {
            // PARTIAL FAIL (Right intent, wrong reason)
            FeedbackManager.Instance.ShowFailure($"You missed the mark. This is a scam using {correctReason}, not {reasonCard.Data.linkedReason}.");
        }
        else
        {
             // FAIL (Safe post)
             FeedbackManager.Instance.ShowFailure("False Alarm! This post is actually safe.");
        }

        // Notify central manager that this post has been handled
        // TODO: Add completion condition (e.g. all phish posts commented on)
        if (BlueTeamManager.Instance != null)
            BlueTeamManager.Instance.NotifyStageComplete();

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
}