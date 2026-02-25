using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class PhishingModuleCard : BaseDraggableCard
{
    public PhishingModule Module { get; private set; }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Image cardBackground;
    
    // Called by ModulePalette to set the data
    public void Initialize(PhishingModule module)
    {
        Module = module;
        if (displayText != null) displayText.text = module.displayText;
        if (statsText != null) 
        {
            statsText.text = $"Success:{Mathf.RoundToInt(module.successModifier * 100)}% \n Suspicion:{Mathf.RoundToInt(module.suspicionModifier * 100)}%";
            // Set colour of card based on level?
            if (cardBackground != null) cardBackground.color = module.cardColour; 
        }
    }
}
