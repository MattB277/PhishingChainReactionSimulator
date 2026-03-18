using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UI;
using System.Linq;

public class ModulePalette : BasePalette
{
    [Header("Data & Prefabs")]
    [SerializeField] private ModuleDatabase database;
    [SerializeField] private GameObject moduleCardPrefab; // ModuleCardPrefab asset

    // Keep track of active cards so we can return them
    private Dictionary<string, PhishingModuleCard> activeCards = new Dictionary<string, PhishingModuleCard>();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PopulatePallete(1); // Populate palette with level 1 cards at first
    }

    public override void PopulatePallete(int level)
    {
        Debug.Log($"Loading Palette for level {level}");
        if (database == null || moduleCardPrefab== null || contentParent == null)
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

        List<PhishingModule> modules = database.GetUnlockedModules(level); // add level modules to palette
        modules.AddRange(database.GetFillerModules()); // add filler modules to palette

        // randomise modules list so they appear in different order each time
        modules = modules.OrderBy(_ => Random.value).ToList();
        // order by hook, then body, then signature (so hooks appear at top of palette, then bodies, then signatures)
        modules = modules.OrderBy(m => m.type).ToList();

        foreach (var moduleData in modules)
        {
            GameObject cardObj = Instantiate(moduleCardPrefab, contentParent);   // create prefab object
            
            // set card data and attach to dictionary
            PhishingModuleCard card = cardObj.GetComponent<PhishingModuleCard>(); 
            if (card != null)
            {
                // set card data
                card.Initialize(moduleData);
                card.HomePalette = contentParent; // Set HomePalette reference for returning cards
                activeCards.Add(moduleData.id, card);
            }
        }
    }
}
