using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public abstract class BaseDropZone : MonoBehaviour, IDropHandler
{
    [Header("Drop Zone Settings")]
    public int maxCapacity = 1;
    
    // Derived classes should use this list to track cards in their drop zone
    protected List<BaseDraggableCard> currentCards = new List<BaseDraggableCard>();

    public virtual void OnDrop(PointerEventData eventData)
    {
        BaseDraggableCard droppedCard = eventData.pointerDrag?.GetComponent<BaseDraggableCard>();
        
        if (droppedCard != null && CanAcceptCard(droppedCard))
        {
            AcceptCard(droppedCard);
        }
        else if (droppedCard != null) // if card exists but has been rejected, return it to its palette
        {
            droppedCard.ReturnToPalette();
        }
    }

    protected virtual bool CanAcceptCard(BaseDraggableCard card)
    {
        return currentCards.Count < maxCapacity;
    }

    protected virtual void AcceptCard(BaseDraggableCard card)
    {
        // Handle moving from another zone
        BaseDropZone previousZone = card.OriginalParent?.GetComponent<BaseDropZone>();
        if (previousZone != null && previousZone != this)
        {
            // Triggers OnCardRemoved in the other zone
            previousZone.RemoveCard(card);
        }

        // Add to this zone
        if (!currentCards.Contains(card)) currentCards.Add(card);

        // Handle Visuals
        card.transform.SetParent(this.transform);
        card.transform.localScale = Vector3.one;
        card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        card.transform.localRotation = Quaternion.identity;

        // Notify Derived Class
        OnCardAdded(card);
    }

    public virtual void RemoveCard(BaseDraggableCard card)
    {
        if (currentCards.Contains(card))
        {
            currentCards.Remove(card);
            OnCardRemoved(card);
        }
    }

    // Virtual methods for red/blue team specific logic when cards are added/removed, like updating stats or sliders.
    protected virtual void OnCardAdded(BaseDraggableCard card) { }
    protected virtual void OnCardRemoved(BaseDraggableCard card) { }
}