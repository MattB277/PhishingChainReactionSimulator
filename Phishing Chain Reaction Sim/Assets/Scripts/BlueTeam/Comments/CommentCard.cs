using UnityEngine;
using TMPro;

public class CommentCard : BaseDraggableCard
{
    public CommentCardData Data { get; private set; }
    [SerializeField] private TextMeshProUGUI bodyText;

    public void Initialize(CommentCardData data)
    {
        Data = data;
        if(bodyText != null) bodyText.text = data.cardText;
    }
}
