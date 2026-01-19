using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Drop zone for phishing module cards
/// Acccepts any module card and manages order based on type
/// </summary>
public class MessageDropZone : MonoBehaviour, IDropHandler
{
    [Header("References")]
    private readonly List<PhishingModuleCard> messageCards = new List<PhishingModuleCard>(); // cards currently in the drop zone
    private PhishingComposer composer; // manager script

    void Awake()
    {
        // find the manager script on the parent 
        composer = GetComponentInParent<PhishingComposer>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        PhishingModuleCard card = eventData.pointerDrag?.GetComponent<PhishingModuleCard>();
        if (card != null)
        {
            PlaceModule(card);
        }
    }

    public void PlaceModule(PhishingModuleCard card)
    {
        // check if card is already in list to prevent duplicates
        if (messageCards.Contains(card))
        {
            messageCards.Remove(card);
        }
        // Set parent to the drop zone
        card.transform.SetParent(this.transform);
        // reset position so card snaps into place correctly
        card.transform.localScale = Vector3.one;
        card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        card.transform.localRotation = Quaternion.identity;
        // add to list and notify composer of update
        messageCards.Add(card);
        NotifyComposer();
    }

    public void RemoveModule(PhishingModuleCard card)
    {
        if (messageCards.Contains(card))
        {
            messageCards.Remove(card);
            composer?.OnModulesChanged();
        }
    }

    // When Clear is clicked on the composer, send all cards back to the ModuleGrid
    public void ClearAllModules()
    {
        ModulePalette palette = FindAnyObjectByType<ModulePalette>();

        // create copy of cards in drop zone
        var cardsToRemove = new List<PhishingModuleCard>(messageCards);

        foreach (var card in cardsToRemove)
        {
            if (palette != null)
            {
                palette.ReturnModule(card);
            }
            else
            {
                // Just destroy objects if palette is missing
                Destroy(card.gameObject);
            }
        }

        messageCards.Clear();
        NotifyComposer();
    }

    // helper to extract just the data from the card list
    // used to calculate stats based on cards in DZ.
    public List<PhishingModule> GetCurrentModuleData()
    {
        List<PhishingModule> dataList = new List<PhishingModule>();

        foreach (var card in messageCards)
        {
            if (card != null && card.Module != null)
            {
                dataList.Add(card.Module);
            }
        }
        return dataList;
    }

    private void NotifyComposer()
    {
        if (composer != null)
        {
            composer.OnModulesChanged();
        }
        else
        {
            Debug.LogWarning("MessageDropZone: Could not find PhishingComposer in parent");
        }
    }
}
