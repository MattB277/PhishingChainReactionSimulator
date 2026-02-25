using UnityEngine;
using UnityEngine.UI;

public abstract class BasePalette : MonoBehaviour
{
    [Header("Base Palette References")]
    [SerializeField] protected Transform contentParent; // The Grid/Scroll Content

    public Transform ContentParent => contentParent;
    public abstract void PopulatePallete(int stage); // Each palette will implement its own population logic based on stage or level
}