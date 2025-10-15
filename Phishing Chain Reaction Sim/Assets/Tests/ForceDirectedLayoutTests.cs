
using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ForceDirectedLayoutTests
{
    private GameObject layoutGameObject;
    private ForceDirectedLayout layout;

    [SetUp]
    public void Setup()
    {
        // create GameObject with ForceDirectedLayout component for each test
        layoutGameObject = new GameObject("TestLayout");
        layout = layoutGameObject.AddComponent<ForceDirectedLayout>();
    }

    [TearDown]
    public void Teardown()
    {
        // clean up after each test
        UnityEngine.Object.DestroyImmediate(layoutGameObject);
    }

    [Test]
    public void IsLayoutComplete_InitiallyFalse()
    {
        Assert.IsFalse(layout.IsLayoutComplete());
    }

    [Test]
    public void RunLayout_WithNullNodes_LogsWarning()
    {
        List<NetworkNode> nullNodes = null;

        layout.RunLayout(nullNodes, 5f);

        Assert.IsFalse(layout.IsLayoutComplete());
        LogAssert.Expect(LogType.Warning, "ForceDirectedLayout: No nodes passed in!");
    }

    [Test]
    public void RunLayout_WithEmptyNodeList_LogsWarning()
    {
        List<NetworkNode> emptyNodes = new List<NetworkNode>();

        layout.RunLayout(emptyNodes, 5f);

        Assert.IsFalse(layout.IsLayoutComplete());
        LogAssert.Expect(LogType.Warning, "ForceDirectedLayout: No nodes passed in!");
    }

    [Test]
    public void RunLayout_WithValidNodes_StartsLayout()
    {
        // Arrange
        List<NetworkNode> nodes = CreateTestNodes(5);

        // Act
        layout.RunLayout(nodes, 5f);

        // Assert
        Assert.IsFalse(layout.IsLayoutComplete()); // Layout should be running
    }

    [Test]
    public void ComponentEnabled_AfterRunLayout()
    {
        List<NetworkNode> nodes = CreateTestNodes(5);
        layout.enabled = false;

        layout.RunLayout(nodes, 5f);

        Assert.IsTrue(layout.enabled); // Should enable Update() calls
    }

    [Test]
    public void CalculateRepulsiveForces_UpdatesNodeVelocities()
    {
        List<NetworkNode> nodes = CreateTestNodes(2);
    
        nodes[0].position = new Vector2(0, 0);
        nodes[1].position = new Vector2(1, 0);

        layout.RunLayout(nodes, 5f);

        // use reflection to access private method
        var method = typeof(ForceDirectedLayout).GetMethod("CalculateRepulsiveForces",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(layout, null);

        // velocities initialized to zero, should be updated to equal, opposite velocities
        Assert.AreNotEqual(Vector2.zero, nodes[0].velocity, "ForceDirectedLayout.CalculateRepulsiveForces(): Node 0 velocity should have changed");
        Assert.AreNotEqual(Vector2.zero, nodes[1].velocity, "ForceDirectedLayout.CalculateRepulsiveForces(): Node 1 velocity should have changed");

        // only the case when layout is of only 2 nodes!
        Assert.AreEqual(nodes[0].velocity, -nodes[1].velocity, "ForceDirectedLayout.CalculateRepulsiveForces(): Node velocities should be equal and opposite");
    }

    [Test]
    public void CalculateRepulsiveForces_IgnoresFarApartNodes()
    {
        List<NetworkNode> nodes = CreateTestNodes(2);
        nodes[0].position = new Vector2(0, 0);
        nodes[1].position = new Vector2(1000, 0);

        layout.RunLayout(nodes, 5f);

        var method = typeof(ForceDirectedLayout).GetMethod("CalculateRepulsiveForces",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(layout, null);

        // nodes too far apart, velocity should remain zero as they have been skipped
        Assert.AreEqual(Vector2.zero, nodes[0].velocity);
        Assert.AreEqual(Vector2.zero, nodes[1].velocity);
    }

    [Test]
    public void CalculateRepulsiveForces_HandlesSamePositionNodes()
    {
        // nodes initialised in helper to vec2(0,0)
        List<NetworkNode> nodes = CreateTestNodes(2);

        layout.RunLayout(nodes, 5f);

        var method = typeof(ForceDirectedLayout).GetMethod("CalculateRepulsiveForces",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(layout, null);

        // nodes should have non-zero velocities due to jitter handling
        Assert.AreNotEqual(Vector2.zero, nodes[0].velocity);
        Assert.AreNotEqual(Vector2.zero, nodes[1].velocity);
    }

    [Test]
    public void CalculateRepulsiveForces_MultipleNodes_ForceDistribution()
    {
        // create a line of 3 nodes with small asymmetric spacing
        List<NetworkNode> nodes = CreateTestNodes(3);
        nodes[0].position = new Vector2(0, 0);      // start node
        nodes[1].position = new Vector2(0.7f, 0);
        nodes[2].position = new Vector2(1, 0);      // end node, middle node is closer to it

        layout.RunLayout(nodes, 5f);

        var method = typeof(ForceDirectedLayout).GetMethod("CalculateRepulsiveForces",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(layout, null);

        // all nodes should have some repulsive velocity
        foreach (var node in nodes)
        {
            Assert.AreNotEqual(Vector2.zero, node.velocity, $"Node {node.id} should have non-zero velocity");
        }

        // nodes closer together have larger velocity magnitude than nodes farther apart
        var endNode = nodes[2]; // end of line
        float endMag = endNode.velocity.magnitude;

        var startNode = nodes[0]; // start node should feel less net force from distant nodes
        float startMag = startNode.velocity.magnitude;

        Assert.Greater(endMag, startMag, "End node should have larger velocity due to closer neighbour");
    }

    [Test]
    public void CalculateAttractiveForces_TwoConnectedNodes_PullTogether()
    {
        List<NetworkNode> nodes = CreateTestNodes(2);
        nodes[0].position = new Vector2(0, 0);
        nodes[1].position = new Vector2(10, 0);

        // connect two nodes
        ConnectTwoNodes(nodes[0], nodes[1]);

        layout.RunLayout(nodes, 5f);

        var method = typeof(ForceDirectedLayout).GetMethod("CalculateAttractiveForces",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(layout, null);

        // nodes should have velocities towards each other
        Assert.AreNotEqual(Vector2.zero, nodes[0].velocity, "Node[0] velocity should have changed.");
        Assert.AreNotEqual(Vector2.zero, nodes[1].velocity, "Node[1] velocity should have changed.");

        // velocities should be equal and opposite
        Assert.AreEqual(nodes[0].velocity, -nodes[1].velocity, "Velocities should be equal and opposite");
    }

    [Test]
    public void CalculateAttractiveForces_NoConnections_NoVelocityChange()
    {
        List<NetworkNode> nodes = CreateTestNodes(2);
        nodes[0].position = new Vector2(0, 0);
        nodes[1].position = new Vector2(1, 0);

        layout.RunLayout(nodes, 5f);

        var method = typeof(ForceDirectedLayout).GetMethod("CalculateAttractiveForces",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(layout, null);

        // velocities should remain zero
        Assert.AreEqual(Vector2.zero, nodes[0].velocity);
        Assert.AreEqual(Vector2.zero, nodes[1].velocity);
    }

    [Test]
    public void CalculateAttractiveForces_StarTopology_AllEdgesCalculated()
    {
        // create one centre node with 4 outer nodes
        List<NetworkNode> nodes = CreateTestNodes(5);
        nodes[0].position = new Vector2(0, 1);
        for (int i = 1; i < 5; i++)
        {
            ConnectTwoNodes(nodes[0], nodes[i]);
            nodes[0].position = new Vector2(i, 0);
        }

        layout.RunLayout(nodes, 10f);

        var method = typeof(ForceDirectedLayout).GetMethod("CalculateAttractiveForces",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        method.Invoke(layout, null);

        // Centre node should feel net force from all 4 connections
        Assert.AreNotEqual(Vector2.zero, nodes[0].velocity, "Centre node should have non-zero velocity");

        // Each outer node should also move
        for (int i = 1; i <= 4; i++)
        {
            Assert.AreNotEqual(Vector2.zero, nodes[i].velocity, $"Outer node {i} should have non-zero velocity");
        }
    }

    [Test]
    public void UpdatePositions_AppliesVelocityToPosition()
    {
        List<NetworkNode> nodes = CreateTestNodes(1);
        nodes[0].position = new Vector2(0, 0);
        nodes[0].velocity = new Vector2(10, 5); // Moving right and up
        
        Vector2 initialPosition = nodes[0].position;
        
        layout.RunLayout(nodes, 5f);
        
        var method = typeof(ForceDirectedLayout).GetMethod("UpdatePositions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(layout, null);
        
        // Position should have changed based on velocity
        Assert.AreNotEqual(initialPosition, nodes[0].position, 
            "Position should change when velocity is applied");
        
        // Position change should be in direction of velocity
        Vector2 displacement = nodes[0].position - initialPosition;
        Assert.Greater(displacement.x, 0, "Should move in positive X direction");
        Assert.Greater(displacement.y, 0, "Should move in positive Y direction");
    }

    [Test]
    public void UpdatePositions_AppliesDamping()
    {
        List<NetworkNode> nodes = CreateTestNodes(1);
        nodes[0].velocity = new Vector2(100, 0); // High velocity

        float initialSpeed = nodes[0].velocity.magnitude;

        layout.RunLayout(nodes, 5f);

        var method = typeof(ForceDirectedLayout).GetMethod("UpdatePositions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        // Apply damping multiple times
        method.Invoke(layout, null);
        float speedAfter1 = nodes[0].velocity.magnitude;

        method.Invoke(layout, null);
        float speedAfter2 = nodes[0].velocity.magnitude;

        // Velocity should decrease each time
        Assert.Less(speedAfter1, initialSpeed, "Velocity should decrease after first damping");
        Assert.Less(speedAfter2, speedAfter1, "Velocity should continue decreasing");
    }

    [Test]
    public void UpdatePositions_ZeroVelocity_NoMovement()
    {
        List<NetworkNode> nodes = CreateTestNodes(1);
        nodes[0].position = new Vector2(5, 5);
        nodes[0].velocity = Vector2.zero;

        Vector2 initialPosition = nodes[0].position;

        layout.RunLayout(nodes, 5f);

        var method = typeof(ForceDirectedLayout).GetMethod("UpdatePositions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(layout, null);

        // Position should not change
        Assert.AreEqual(initialPosition, nodes[0].position,
            "UpdatePosition should not change node position with zero velocity");
    }

    public void ConnectTwoNodes(NetworkNode a, NetworkNode b)
    {
        NetworkEdge edge = new NetworkEdge(a, b, 1f, EdgeType.Colleague);
        a.connections.Add(edge);
        NetworkEdge inverseEdge = new NetworkEdge(b, a, 1f, EdgeType.Colleague);
        b.connections.Add(inverseEdge);
    }

    private List<NetworkNode> CreateTestNodes(int nodeCount)
    {
        List<NetworkNode> nodes = new List<NetworkNode>();

        // nodes all initialised to position 0,0
        for (int i = 0; i < nodeCount; i++)
        {
            NetworkNode node = new NetworkNode(i);
            node.position = new Vector2(0, 0);

            nodes.Add(node);
        }
        return nodes;
    }



    // A Test behaves as an ordinary method
    [Test]
    public void ForceDirectedLayoutTestsSimplePasses()
    {
        // Use the Assert class to test conditions
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator ForceDirectedLayoutTestsWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
