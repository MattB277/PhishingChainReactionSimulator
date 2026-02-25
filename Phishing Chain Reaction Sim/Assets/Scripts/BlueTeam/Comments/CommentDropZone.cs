using UnityEngine;

public class CommentDropZone : BaseDropZone
{
    // Set in Inspector (Categorization, Reasoning, or Advice)
    [SerializeField]
    [Header("Category of comment card this drop zone accepts")]
    private CardCategory acceptedCategory; 

    void Awake()
    {
        maxCapacity = 1; // Enforce single card rule
    }

    // Override the base check to also check if the card is the right category of drop zone
    protected override bool CanAcceptCard(BaseDraggableCard card)
    {
        CommentCard cCard = card as CommentCard;
        
        // If cast fails, reject.
        // Could also make this swap the cards over?
        if (cCard == null) return false;

        // Check 1: Is there space? (Use 'currentCards' from Base Class)
        // Check 2: Does the category match?
        return currentCards.Count < maxCapacity && cCard.Data.category == acceptedCategory;
    }

    public CommentCard GetCardInZone()
    {
        if (currentCards.Count > 0)
        {
            return currentCards[0] as CommentCard;
        }
        return null;
    }

    // Move the cards home and clear the list so the zone is ready for new input
    public void ClearZone()
    {
        if (currentCards.Count > 0)
        {
            currentCards[0].ReturnToPalette(); // Handles both logic and visuals
        }
    }
}