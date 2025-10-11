using System.Collections.Generic;
using UnityEngine;

public class GraphManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject nodePrefab;

    [Header("Graph Settings")]
    [SerializeField] private int nodeCount = 20;
    [SerializeField] private float connectionProbability = 0.15f;

    [Header("Visual Settings")]
    [SerializeField] private float graphRadius = 5f;
    [SerializeField] private Color cleanColor = Color.green;
    [SerializeField] private Color infectedColor = Color.red;
    [SerializeField] private float lineWidth = 0.2f;

    [Header("Layout")]
    [SerializeField] private ForceDirectedLayout layout;

    private List<NetworkNode> nodes = new List<NetworkNode>();
    private List<LineRenderer> edgeRenderers = new List<LineRenderer>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateGraph();
        RenderGraph();
        layout.RunLayout(nodes, graphRadius);        
    }

    // initialise nodeCount nodes, with edges and initial positions. 
    void GenerateGraph()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            NetworkNode node = new NetworkNode(i);

            // nodes are initialised on the circumference
            float angle = i / (float)nodeCount * Mathf.PI * 2; // evenly space nodes
            node.position = new Vector2(                        // convert polar to cartesian coordinates
                Mathf.Cos(angle) * graphRadius,
                Mathf.Sin(angle) * graphRadius
            );

            nodes.Add(node);
        }
        Debug.Log($"Created {nodes.Count} nodes");

        // Create random connections
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                if (Random.value < connectionProbability)
                {
                    NetworkEdge edge = new NetworkEdge(
                        nodes[i],
                        nodes[j],
                        Random.Range(0.3f, 1f),
                        EdgeType.Colleague
                    );

                    nodes[i].connections.Add(edge);

                    // add reverse connection
                    NetworkEdge reverseEdge = new NetworkEdge(
                        nodes[j],
                        nodes[i],
                        edge.relationshipStrength,
                        edge.type
                    );
                    nodes[j].connections.Add(reverseEdge);
                }
            }
        }

        Debug.Log($"Generated graph with {nodes.Count} and {CountEdges()} edges");
    }

    void RenderGraph()
    {
        // Render nodes
        foreach (NetworkNode node in nodes)
        {
            GameObject nodeObj = Instantiate(nodePrefab, node.position, Quaternion.identity, transform);
            node.gameObject = nodeObj;

            SpriteRenderer sr = nodeObj.GetComponent<SpriteRenderer>();
            sr.color = GetNodeColor(node.state);
        }

        // Render edges
        foreach (NetworkNode node in nodes)
        {
            foreach (NetworkEdge edge in node.connections)
            {
                // only render edges once!
                if (edge.source.id < edge.target.id)
                {
                    CreateEdgeLine(edge);
                }
            }
        }
    }


    void CreateEdgeLine(NetworkEdge edge)
    {
        GameObject edgeObj = new GameObject($"Edge_{edge.source.id}_{edge.target.id}");
        edgeObj.transform.parent = transform;

        LineRenderer lr = edgeObj.AddComponent<LineRenderer>();
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.gray;
        lr.endColor = Color.gray;
        lr.sortingOrder = -1; // draw lines behind nodes

        lr.positionCount = 2;
        lr.SetPosition(0, edge.source.position);
        lr.SetPosition(1, edge.target.position);

        edgeRenderers.Add(lr);
    }

    Color GetNodeColor(NodeState state)
    {
        switch (state)
        {
            case NodeState.Clean: return cleanColor;
            case NodeState.Infected: return infectedColor;
            case NodeState.Aware: return Color.blue;
            case NodeState.Quarantined: return Color.yellow;
            default: return Color.white;
        }
    }

    int CountEdges()
    {
        int count = 0;
        foreach (NetworkNode node in nodes)
        {
            count += node.connections.Count;
        }
        return count / 2; // dont count nodes twice
    }

    // Update is called once per frame
    void Update()
    {
        // update edge positions each frame (obselete in future builds)
        int edgeIndex = 0;
        foreach (NetworkNode node in nodes)
        {
            foreach (NetworkEdge edge in node.connections)
            {
                if (edge.source.id < edge.target.id)
                {
                    if (edgeIndex < edgeRenderers.Count)
                    {
                        LineRenderer lr = edgeRenderers[edgeIndex];
                        lr.SetPosition(0, edge.source.position);
                        lr.SetPosition(1, edge.target.position);
                    }
                    edgeIndex++;
                }
            }
        }
    }
}
