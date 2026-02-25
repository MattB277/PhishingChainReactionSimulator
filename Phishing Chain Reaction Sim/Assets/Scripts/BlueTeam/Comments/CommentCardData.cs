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
    public string cardText; // text shown to user on the card
    public CardCategory category; // Categorisation, Reasoning, Advice
    public PhishReason linkedReason; // Only for Reasoning Cards, link to specific phish reason for answer checking
}

