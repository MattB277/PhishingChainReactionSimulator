using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PhishingModuleCard : MonoBehaviour
{
    public PhishingModule Module { get; private set; }

    [Header("References")]
    [SerializeField] private Text displayText;
    [SerializeField] private Text statsText;
    [SerializeField] private Image cardBackground;
    
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>(); 
    }

    // Called by ModulePalette to set the data
    public void Initialize(PhishingModule module)
    {
        Module = module;
        if (displayText != null) displayText.text = module.displayText;
        if (statsText != null) 
        {
            statsText.text = $"S:{Mathf.RoundToInt(module.successModifier * 100)}% / R:{Mathf.RoundToInt(module.suspicionModifier * 100)}%";
            // Customize visual properties based on data
            if (cardBackground != null) cardBackground.color = module.cardColour; 
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
