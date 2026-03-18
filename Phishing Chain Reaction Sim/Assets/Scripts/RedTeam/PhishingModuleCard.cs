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
    [SerializeField] private TextMeshProUGUI cardType;
    [SerializeField] private Image cardBackground;
    
    // Called by ModulePalette to set the data
    public void Initialize(PhishingModule module)
    {
        Module = module;
        if (displayText != null) displayText.text = module.displayText;
        if (cardType != null) cardType.text = module.type.ToString();
        // Set colour of card based on level?
        if (cardBackground != null) cardBackground.color = module.cardColour; 
    }
}

