using System.Collections.Generic;
using UnityEngine;


public enum NodeState
{
    Clean,
    Infected,
    Aware,
    Quarantined
}
public class NetworkNode
{
    public int id;
    public Vector2 position;
    public Vector2 velocity;
    public NodeState state;
    public float awarenessLevel;
    public List<NetworkEdge> connections;
    public GameObject gameObject; // link node to visual representation

    public NetworkNode(int id)
    {
        this.id = id;
        this.position = Vector2.zero;
        this.velocity = Vector2.zero;
        this.state = NodeState.Clean;
        this.awarenessLevel = 0.5f;
        this.connections = new List<NetworkEdge>();
    }
}
