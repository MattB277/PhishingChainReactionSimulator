using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class PhishingModuleCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public PhishingModule Module { get; private set; }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Image cardBackground;
    
    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Transform originalParent;  
    private int originalSiblingIdx; // Keep location in ModuleGrid

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        Canvas[] canvases = GetComponentsInParent<Canvas>();
        if (canvases.Length > 0)
        {
            canvas = canvases[canvases.Length -1];
        }
    }

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

    public void OnBeginDrag(PointerEventData eventData)
        {
            originalParent = transform.parent; 
            originalSiblingIdx = transform.GetSiblingIndex();

            transform.SetParent(canvas.transform, true); // Pull out of scrollview and into root canvas

            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false; 
        }

        public void OnDrag(PointerEventData eventData)
        {
            transform.position = eventData.position; // move to exact mouse position while dragging

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

            MessageDropZone originalDropZone = originalParent.GetComponent<MessageDropZone>();

            if (originalDropZone != null)
                {
                    // clean up list inside dropZone before returning to palette
                    originalDropZone.RemoveModule(this);
                }
            
            ModulePalette palette = FindFirstObjectByType<ModulePalette>();

            if (palette != null)
            {
                // palette handles setting parent back to ModuleGrid
                palette.ReturnModule(this);
            }
        }
}
