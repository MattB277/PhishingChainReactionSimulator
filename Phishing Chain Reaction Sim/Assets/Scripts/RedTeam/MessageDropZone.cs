using UnityEngine;
using System.Collections.Generic;

public class MessageDropZone : BaseDropZone
{
    private PhishingComposer composer; 

    void Awake()
    {
        composer = GetComponentInParent<PhishingComposer>();
        maxCapacity = 6;
    }

    protected override void OnCardAdded(BaseDraggableCard card)
    {
        // Tell the manager to recalculate stats based on new card.
        NotifyComposer();
    }

    protected override bool CanAcceptCard(BaseDraggableCard card)
    {
        // Keep base capacity behavior.
        if (currentCards.Count >= maxCapacity) return false;

        // Only module cards are valid for this drop zone.
        PhishingModuleCard incomingCard = card as PhishingModuleCard;
        if (incomingCard == null || incomingCard.Module == null) return false;

        ModuleType incomingType = incomingCard.Module.type;

        // Reject if a card with the same module type already exists in this zone.
        foreach (BaseDraggableCard existingCard in currentCards)
        {
            PhishingModuleCard pCard = existingCard as PhishingModuleCard;
            if (pCard != null && pCard.Module != null && pCard.Module.type == incomingType)
            {
                return false;
            }
        }

        return true;
    }

    protected override void OnCardRemoved(BaseDraggableCard card)
    {
        // Tell the manager to recalculate stats based on removed card.
        NotifyComposer();
    }

    public void ClearAllModules()
    {
        // Copy list since ReturnToPalette will modify currentCards via RemoveCard
        var cardsToRemove = new List<BaseDraggableCard>(currentCards);

        foreach (var card in cardsToRemove)
        {
            card.ReturnToPalette(); // Handles RemoveCard + visual reparent
        }

        // Single notification after all cards are cleared
        NotifyComposer();
    }

    public List<PhishingModule> GetCurrentModuleData()
    {
        List<PhishingModule> dataList = new List<PhishingModule>();

        foreach (var baseCard in currentCards)
        {
            if (baseCard is PhishingModuleCard pCard && pCard.Module != null)
            {
                dataList.Add(pCard.Module);
            }
        }
        return dataList;
    }

    private void NotifyComposer()
    {
        if (composer != null) composer.OnModulesChanged();
    }
}