using UnityEngine;
using System.Collections.Generic;

// implement force directed graph layout using Fruchterman-Reinhold algorithm
public class ForceDirectedLayout : MonoBehaviour
{
    [Header("Force Parameters")]
    [SerializeField] private float attractionStrength = 1.0f;
    [Tooltip("Strength of attraction forces between connected nodes")]
    
    [SerializeField] private float repulsionStrength = 10f;
    [Tooltip("Strength of repulsive forces between all nodes")]
    
    [SerializeField] private float damping = 0.85f;
    [Tooltip("Velocity damping factor (0-1). Higher = slower settling")]

    [Header("Scaling")]
    [SerializeField] private bool autoScale = true;
    [Tooltip("Automatically calculate optimal distance based on network size")]
    
    [SerializeField] private float baseOptimalDistance = 3f;
    [Tooltip("Base optimal spacing between nodes (k constant)")]

    [Header("Convergence")]
    [SerializeField] private int maxIterations = 300;
    [Tooltip("Maximum layout iterations before forcing stop")]
    
    [SerializeField] private float convergenceThreshold = 0.01f;
    [Tooltip("Max velocity magnitude for convergence detection")]

    [Header("Debug Visualization")]
    [SerializeField] private bool showAnimation = true;
    [Tooltip("Show nodes moving during layout.")]
    
    [SerializeField] private int iterationsPerFrame = 5;
    [Tooltip("Layout iterations per frame when animating")]
    
    [SerializeField] private bool showDebugInfo = true;
    [Tooltip("Display iteration count and convergence info")]
    
}
