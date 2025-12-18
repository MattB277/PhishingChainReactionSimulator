using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

// implement force directed graph layout using Fruchterman-Reingold algorithm
public class ForceDirectedLayout : MonoBehaviour
{
    [Header("Force Parameters")]
    [SerializeField] private float attractionStrength = 1.0f;
    [Tooltip("Strength of attraction forces between connected nodes")]
    
    [SerializeField] private float repulsionStrength = 10f;
    [Tooltip("Strength of repulsive forces between all nodes")]

    [SerializeField] private float repulsionCutoffMultiplier = 3f;
    [Tooltip("Ignore repulsion beyond this multiple of optimal distance")]
    
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

    private List<NetworkNode> nodes;
    private bool isLayoutComplete = false;
    private float maxVelocity = float.MinValue;
    private int currentIteration = 0;
    private float k; // optimal distance constant

    public void RunLayout(List<NetworkNode> nodes, float graphRadius)
    {
        // check nodes exist
        if (nodes == null || nodes.Count == 0)
        {
            Debug.LogWarning("ForceDirectedLayout: No nodes passed in!");
            return;
        }

        this.nodes = nodes;
        isLayoutComplete = false;
        currentIteration = 0;

        // calculate optimal distance if auto-scale enabled
        if (autoScale) CalculateOptimalDistance(graphRadius);

        enabled = true; // enable update() calls

        Debug.Log($"ForceDirectedLayout: Starting layout for {nodes.Count} nodes, with k={k:F2}");
    }

    // calculate optimal distance constant based on graph size
    // Formula: k = C x sqrt(area/nodeCount)
    private void CalculateOptimalDistance(float graphRadius)
    {
        float area = Mathf.PI * graphRadius * graphRadius;
        k = baseOptimalDistance * Mathf.Sqrt(area / nodes.Count);
    }

    void Update()
    {
        if (isLayoutComplete) return;

        if (showAnimation)
        {
            // Animation enabled
            for (int i = 0; i < iterationsPerFrame; i++)
            {
                PerformIteration();
            }
            SyncVisualPositions();
        }
        else
        {
            // No visual updates between iterations
            while (!isLayoutComplete)
            {
                PerformIteration();
            }
            SyncVisualPositions();
        }
    }

    // perform one iteration of the force directed algorithm
    private void PerformIteration()
    {
        if (currentIteration % 10 == 0 && showDebugInfo ) // Every 10 iterations
            {
                Debug.Log($"Iteration {currentIteration}");
            }
        // apply all forces
        CalculateRepulsiveForces();
        CalculateAttractiveForces();

        maxVelocity = 0f;
        // update positions based on new velocities and update max velocity
        UpdatePositions();

        currentIteration++;

        // Check convergence or max iterations reached
        if (maxVelocity < convergenceThreshold || currentIteration >= maxIterations)
        {
            isLayoutComplete = true;
            enabled = false;

            string reason = currentIteration >= maxIterations ? "max iterations" : "converged";
            Debug.Log($"ForceDirectedLayout: Completed layout in {currentIteration} iterations ({reason})");
        }
    }
    private void SyncVisualPositions()
    {
        foreach (NetworkNode node in nodes)
        {
            if (node.gameObject != null)
            {
                node.gameObject.transform.position = node.position;
            }
        }
    }

    // Calculate repulsive forces between all node pairs, using distance cutoff and squared distance optimisations. 
    // TODO: map formulas to code because this is a lot of maths
    private void CalculateRepulsiveForces()
    {
        // repulsive force: F = k^2 / distance
        // calculated as F = repulsionStrength * k * k / distance

        float cutoffDistance = k * repulsionCutoffMultiplier;
        float cutoffDistanceSq = cutoffDistance * cutoffDistance; // calculate square as cuttoff based on square distance

        for (int i = 0; i < nodes.Count; i++)
        {

            for (int j = 0; j < nodes.Count; j++)
            {
                if (i == j) continue; // dont repel from self

                // calculate position delta
                Vector2 delta = nodes[i].position - nodes[j].position;
                // use squared distance for faster cutoff and same pos checks (no square root when it isnt needed)
                float distanceSq = delta.sqrMagnitude;

                // skip far away pairs
                if (distanceSq > cutoffDistanceSq) continue;

                // handle same pos edge case by adding small jitter to existing position
                if (distanceSq < 0.0001f)
                {
                    delta = (UnityEngine.Random.insideUnitCircle.normalized + Vector2.one * 0.001f) * 0.01f;
                    distanceSq = delta.sqrMagnitude;
                }

                // convert squared distance to true euclid distance
                float distance = Mathf.Sqrt(distanceSq);

                // calculate force magnitude
                float forceMagnitude = repulsionStrength * k * k / distance;

                // calculate forceVector
                Vector2 forceDirection = delta / distance;
                Vector2 forceVector = forceDirection * forceMagnitude;

                nodes[i].velocity += forceVector;
                nodes[j].velocity -= forceVector;
            }
        }
    }

    // calculate attractive forces between connected nodes
    // stronger as distance increases
    private void CalculateAttractiveForces()
    {
        // attractive force: F = distance^2 / k
        // calculated as F = attractionStrength * distance * distance / k;
        
        foreach (NetworkNode node in nodes)
        {
            foreach (NetworkEdge edge in node.connections)
            {
                // skip reverse edges
                if (edge.source.id >= edge.target.id) continue;

                Vector2 delta = edge.target.position - edge.source.position;
                float distanceSq = delta.sqrMagnitude;

                // skip same position pairs (shouldn't happen unless first iteration)
                if (distanceSq < 0.0001f) continue; 

                // convert squared distance to true euclid distance
                float distance = Mathf.Sqrt(distanceSq);

                // calculate force magnitude
                float forceMagnitude = attractionStrength * distance * distance / k;

                // calculate forceVector
                Vector2 forceDirection = delta / distance;
                Vector2 forceVector = forceDirection * forceMagnitude;

                edge.source.velocity += forceVector; // Pull source toward target
                edge.target.velocity -= forceVector; // Pull target toward source
            }
        }
    }

    // update logical node positions and find maxVelocity for this iteration
    private void UpdatePositions()
    {
        foreach (NetworkNode node in nodes)
        {
            // track max velocity for this iteration
            if (node.velocity.magnitude > maxVelocity)
            {
                maxVelocity = node.velocity.magnitude;
            }

            // * by deltaTime for displacement
            node.position += node.velocity * Time.deltaTime;
            // 0.85 damping reduces velocity by 15% each frame
            node.velocity *= damping;
        }
    }

    public bool IsLayoutComplete()
    {
        return isLayoutComplete;
    }

}
