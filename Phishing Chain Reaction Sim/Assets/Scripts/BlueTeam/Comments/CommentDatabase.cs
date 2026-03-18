using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CommentDatabase", menuName = "BlueTeam/CommentDatabase")]
public class CommentDatabase : ScriptableObject
{
    public List<CommentCardData> allCards;

    // Helper method to get cards specific to a stage or category if needed later
    public List<CommentCardData> GetCardsByCategory(CardCategory category)
    {
        return allCards.Where(card => card.category == category).ToList();
    }

    public List<CommentCardData> GetAllCards()
    {
        return allCards;
    }
}
