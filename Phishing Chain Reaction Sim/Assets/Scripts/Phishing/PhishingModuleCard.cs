using UnityEngine;
using UnityEngine.EventSystems;
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
    public void OnBeginDrag(PointerEventData eventData)
        {
            originalParent = transform.parent; 
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false; 
            transform.SetParent(canvas.transform); // Render on top
        }

        public void OnDrag(PointerEventData eventData)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            if (eventData.pointerEnter != null)
            {
                MessageDropZone dropZone = eventData.pointerEnter.GetComponentInParent<MessageDropZone>();
                // If dropped on the MessageDropZone, place the module there
                if (dropZone != null)
                {
                    dropZone.PlaceModule(this);
                    return;
                }
            }
            
            // If drop failed, find the palette to return the card
            ModulePalette palette = FindObjectOfType<ModulePalette>();
            palette?.ReturnModule(this); 
        }

    // Update is called once per frame
    void Update()
    {
        
    }
}
