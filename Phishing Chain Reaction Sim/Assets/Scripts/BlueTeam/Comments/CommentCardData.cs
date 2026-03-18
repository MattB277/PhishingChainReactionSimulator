using UnityEngine;

public enum CardCategory
{
    Categorisation,
    Reasoning,
    Advice
}

[System.Serializable]
public class CommentCardData
{
    public string id;
    public string cardText;
    public CardCategory category;

    [Header("Link one field per category")]
    public PhishImpactType linkedImpact;  // Categorisation cards: what type of phish
    public PhishReason    linkedReason;   // Reasoning cards:      the giveaway
    public PhishAdvice    linkedAdvice;   // Advice cards:          defence tip (must match the giveaway)
}

