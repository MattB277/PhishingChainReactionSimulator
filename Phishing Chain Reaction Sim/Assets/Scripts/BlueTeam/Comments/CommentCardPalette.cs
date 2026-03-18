using System.Collections.Generic;
using UnityEngine;

public class CommentCardPalette : BasePalette
{
    [Header("Comment Palette Settings")]
    [SerializeField] private GameObject commentCardPrefab;
    [SerializeField] private CommentDatabase database;
    
    public override void PopulatePallete(int stage = 0)
    {
        Debug.Log($"[CommentCardPalette] PopulatePallete called. Database: {database}, Prefab: {commentCardPrefab}, ContentParent: {contentParent}");

        // clear existing cards
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // populate with new cards
        foreach (CommentCardData data in database.GetAllCards())
        {
            GameObject newCard = Instantiate(commentCardPrefab, contentParent);
            CommentCard cardComponent = newCard.GetComponent<CommentCard>();
            if (cardComponent != null)
            {
                cardComponent.Initialize(data);
                cardComponent.HomePalette = contentParent; // pass palette along to card
            }
            else
            {
                Debug.LogError($"[CommentCardPalette] Prefab '{commentCardPrefab.name}' is missing a CommentCard component!", newCard);
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
