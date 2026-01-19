using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;

public class ModulePalette : MonoBehaviour
{
    [Header("Data & Prefabs")]
    [SerializeField] private ModuleDatabase database;
    [SerializeField] private GameObject moduleCardPrefab; // ModuleCardPrefab asset
    
    [Header("UI References")]
    [SerializeField] private Transform moduleGridParent; // ModuleGrid panel

    // Keep track of active cards so we can return them
    private Dictionary<string, PhishingModuleCard> activeCards = new Dictionary<string, PhishingModuleCard>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PopulatePallete(1); // Populate palette with level 1 cards at first
    }

    public void PopulatePallete(int level)
    {
        Debug.Log($"Loading Palette for level {level}");
        if (database == null || moduleCardPrefab== null || moduleGridParent == null)
        {
            Debug.LogError("ModulePallete dependancies are not set in Inspector");
        }

        // Remove old cards
        foreach (var card in activeCards.Values)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        activeCards.Clear(); // clear active cards dictionary

        List<PhishingModule> modules = database.GetUnlockedModules(level);

        foreach (var moduleData in modules)
        {
            GameObject cardObj = Instantiate(moduleCardPrefab, moduleGridParent);   // create prefab object
            
            // set card data and attach to dictionary
            PhishingModuleCard card = cardObj.GetComponent<PhishingModuleCard>(); 
            if (card != null)
            {
                // set card data
                card.Initialize(moduleData);
                activeCards.Add(moduleData.id, card);
            }
        }
    }

    public void ReturnModule(PhishingModuleCard card)
    // handles when a card is dropped outside of the dropZone and re-parents the card back to the moduleGrid
    {
        // Remove from messageDropZone if card exists there
        MessageDropZone dropZone = FindFirstObjectByType<MessageDropZone>();
        dropZone?.RemoveModule(card);

        // reparent card to moduleGrid
        card.transform.SetParent(moduleGridParent);
        card.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        // refresh layout group
        LayoutRebuilder.ForceRebuildLayoutImmediate(moduleGridParent.GetComponent<RectTransform>());
    }
}
