using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public abstract class BaseDraggableCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    protected Canvas canvas;
    protected CanvasGroup canvasGroup;
    protected RectTransform rectTransform;

    public Transform HomePalette { get; set; } // Set when card is created by palette, used for returning cards to palette when rejected or cleared
    public Transform OriginalParent { get; private set; }
    public int OriginalSiblingIndex { get; private set; }

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        Canvas[] canvases = GetComponentsInParent<Canvas>();
        if (canvases.Length > 0)
        {
            canvas = canvases[canvases.Length - 1]; // Get the topmost canvas in the hierarchy
        }
        else
        {
            Debug.LogError("No Canvas found in parent hierarchy.");
        }
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        OriginalParent = transform.parent;
        OriginalSiblingIndex = transform.GetSiblingIndex();

        // Logically remove from zone before dragging, fixes bug of duplicated logical cards when dropped back into zone they were already in.
        BaseDropZone zone = OriginalParent?.GetComponent<BaseDropZone>();
        if (zone != null)
        {
            zone.RemoveCard(this);
        }

        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (transform.parent == canvas.transform)
        {
           ReturnToPalette();
        }
    }

    public virtual void ReturnToPalette()
    {
        // Logically remove from any drop zone the card is currently in
        // Check OriginalParent first (valid during a drag when parent is canvas)
        // Fall back to current parent (valid when called outside of drag, e.g. Clear button)
        BaseDropZone zone = OriginalParent?.GetComponent<BaseDropZone>();
        if (zone == null)
        {
            zone = transform.parent?.GetComponent<BaseDropZone>();
        }

        if (zone != null)
        {
            zone.RemoveCard(this);
        }

        // 2. Visually return to palette
        if (HomePalette != null)
        {
            transform.SetParent(HomePalette);
            transform.localScale = Vector3.one;
            rectTransform.anchoredPosition = Vector2.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}
