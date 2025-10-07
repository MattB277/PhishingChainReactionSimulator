using UnityEngine;

public enum EdgeType
{
    Colleague,
    DirectReport,
    Manager,
    FrequentContact
}

public class NetworkEdge
{
    public NetworkNode source;
    public NetworkNode target;
    public float relationshipStrength;  // trust level between two nodes, higher = more susceptible
    public EdgeType type;

    public NetworkEdge(NetworkNode source, NetworkNode target, float strength, EdgeType type)
    {
        this.source = source;
        this.target = target;
        this.relationshipStrength = strength;
        this.type = type;
    }
}
