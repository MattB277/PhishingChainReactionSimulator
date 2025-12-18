using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ScriptableObject database to hold all available phishing module cards.
/// Create new ones through Assets/Create/Phishing/Module Database 
/// </summary>

[CreateAssetMenu(fileName = "ModuleDatabase", menuName = "Phishing/ModuleDatabase")]
public class ModuleDatabase : ScriptableObject
{
    public List<PhishingModule> allModules = new List<PhishingModule>();

    // Get all modules unlocked by given level
    public List<PhishingModule> GetUnlockedModules(int currentLevel)
    {
        return allModules.Where(m => m.unlockLevel <= currentLevel).ToList();
    }

    // Get modules of a given type that are unlocked.
    public List<PhishingModule> GetModulesByType(ModuleType type, int currentLevel)
    {
        return allModules.Where(m => m.type == type && m.unlockLevel <= currentLevel).ToList();
    }

    // Get a module by its ID
    public PhishingModule GetModuleByID(string id)
    {
        return allModules.FirstOrDefault(m => m.id == id);
    }

    // Get count of modules for a given type
    public int GetModuleCountByType(ModuleType type)
    {
        return allModules.Count(m => m.type == type);
    }
}
