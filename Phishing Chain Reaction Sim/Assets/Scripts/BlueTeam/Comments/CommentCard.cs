using UnityEngine;
using TMPro;

public class CommentCard : BaseDraggableCard
{
    public CommentCardData Data { get; private set; }
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private TextMeshProUGUI TypeText; // type of card 
    public void Initialize(CommentCardData data)
    {
        Data = data;

        if (bodyText == null)
        {
            Debug.LogError($"[CommentCard] bodyText reference is missing on '{gameObject.name}'. Check the prefab.", this);
            return;
        }

        if (TypeText == null)
        {
            Debug.LogError($"[CommentCard] TypeText reference is missing on '{gameObject.name}'. Check the prefab.", this);
            return;
        }

        bodyText.text = data.cardText;
        TypeText.text = data.category.ToString();

        Debug.Log($"[CommentCard] Initialized '{data.id}' — body: '{data.cardText}', category: {data.category}");
    }
}
