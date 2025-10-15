
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

    [Test]
    public void SyncVisualPositions_UpdatesGameObjectTransforms()
    {
        List<NetworkNode> nodes = CreateTestNodes(2);

        // Create GameObjects for nodes
        GameObject obj1 = new GameObject("Node0");
        GameObject obj2 = new GameObject("Node1");
        nodes[0].gameObject = obj1;
        nodes[1].gameObject = obj2;

        // Set node data positions
        nodes[0].position = new Vector2(5, 3);
        nodes[1].position = new Vector2(-2, 7);

        layout.RunLayout(nodes, 5f);

        var method = typeof(ForceDirectedLayout).GetMethod("SyncVisualPositions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(layout, null);

        Assert.AreEqual(nodes[0].position, (Vector2)obj1.transform.position,
            "GameObject position should match node data position");
        Assert.AreEqual(nodes[1].position, (Vector2)obj2.transform.position,
            "GameObject position should match node data position");

        // Cleanup game objects
        UnityEngine.Object.DestroyImmediate(obj1);
        UnityEngine.Object.DestroyImmediate(obj2);
    }

    [Test]
    public void SyncVisualPositions_NullGameObject_NoError()
    {
        // Node with null GameObject
        List<NetworkNode> nodes = CreateTestNodes(1);
        nodes[0].gameObject = null;
        nodes[0].position = new Vector2(5, 5);

        layout.RunLayout(nodes, 5f);

        var method = typeof(ForceDirectedLayout).GetMethod("SyncVisualPositions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Should not throw exception
        Assert.DoesNotThrow(() => method.Invoke(layout, null),
            "Should handle null GameObject gracefully");
    }

    [Test]
    public void SyncVisualPositions_MixedNullAndValid_HandlesCorrectly()
    {
        // Some nodes with GameObjects, some without
        List<NetworkNode> nodes = CreateTestNodes(3);

        GameObject obj1 = new GameObject("Node0");
        nodes[0].gameObject = obj1;
        nodes[0].position = new Vector2(1, 1);

        nodes[1].gameObject = null; // No GameObject
        nodes[1].position = new Vector2(2, 2);

        GameObject obj3 = new GameObject("Node2");
        nodes[2].gameObject = obj3;
        nodes[2].position = new Vector2(3, 3);

        layout.RunLayout(nodes, 5f);

        var method = typeof(ForceDirectedLayout).GetMethod("SyncVisualPositions",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method.Invoke(layout, null);

        // Valid GameObjects should be updated
        Assert.AreEqual(nodes[0].position, (Vector2)obj1.transform.position);
        Assert.AreEqual(nodes[2].position, (Vector2)obj3.transform.position);

        // Cleanup game objects
        UnityEngine.Object.DestroyImmediate(obj1);
        UnityEngine.Object.DestroyImmediate(obj3);
    }

    [Test]
    public void PerformIteration_ConvergesWhenVelocityBelowThreshold()
    {
        // Nodes with very low velocities (near convergence)
        List<NetworkNode> nodes = CreateTestNodes(2);
        nodes[0].velocity = new Vector2(0.005f, 0);
        nodes[1].velocity = new Vector2(0.003f, 0);
        ConnectTwoNodes(nodes[0], nodes[1]);

        layout.RunLayout(nodes, 5f);

        var method = typeof(ForceDirectedLayout).GetMethod("PerformIteration",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Perform several iterations
        for (int i = 0; i < 10; i++)
        {
            method.Invoke(layout, null);
            if (layout.IsLayoutComplete()) break;
        }

        // Should converge quickly with low velocities
        Assert.IsTrue(layout.IsLayoutComplete(),
            "Layout should converge when velocities are below threshold");
    }

    [Test]
    public void PerformIteration_StopsAtMaxIterations()
    {
        // Nodes that won't converge (keep adding velocity)
        List<NetworkNode> nodes = CreateTestNodes(2);
        nodes[0].position = new Vector2(0, 0);
        nodes[1].position = new Vector2(0.5f, 0); // Close together for strong repulsion

        layout.RunLayout(nodes, 5f);

        var method = typeof(ForceDirectedLayout).GetMethod("PerformIteration",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Run exactly maxIterations (default 300)
        for (int i = 0; i < 300; i++)
        {
            if (layout.IsLayoutComplete()) break;
            method.Invoke(layout, null);
        }

        // Should stop at max iterations
        Assert.IsTrue(layout.IsLayoutComplete(),
            "Layout should stop after max iterations");
    }

    [Test]
    public void SingleNodeGraph_NoForces_NoMovement()
    {
        // Arrange
        List<NetworkNode> nodes = CreateTestNodes(1);
        nodes[0].position = new Vector2(5, 5);
        Vector2 initialPosition = nodes[0].position;

        layout.RunLayout(nodes, 5f);

        // Run both force calculations
        var repelMethod = typeof(ForceDirectedLayout).GetMethod("CalculateRepulsiveForces",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attractMethod = typeof(ForceDirectedLayout).GetMethod("CalculateAttractiveForces",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        repelMethod.Invoke(layout, null);
        attractMethod.Invoke(layout, null);

        // Single node should have no forces applied
        Assert.AreEqual(Vector2.zero, nodes[0].velocity,
            "Single node should have zero velocity (no forces)");
    }

    [Test]
    public void LargeGraph_PerformanceAcceptable()
    {
        // Create a larger graph (100 nodes)
        List<NetworkNode> nodes = CreateTestNodes(100);

        // Spread nodes out to avoid all being at origin
        for (int i = 0; i < nodes.Count; i++)
        {
            float angle = (i / (float)nodes.Count) * Mathf.PI * 2f;
            nodes[i].position = new Vector2(
                Mathf.Cos(angle) * 10f,
                Mathf.Sin(angle) * 10f
            );
        }

        layout.RunLayout(nodes, 15f);

        var method = typeof(ForceDirectedLayout).GetMethod("CalculateRepulsiveForces",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Measure time for one iteration
        var startTime = System.Diagnostics.Stopwatch.StartNew();
        method.Invoke(layout, null);
        startTime.Stop();

        // Should complete in reasonable time (< 100ms for 100 nodes)
        Assert.Less(startTime.ElapsedMilliseconds, 100,
            "100-node repulsion calculation should take < 100ms");
    }

    [Test]
    public void DisconnectedGraph_OnlyRepulsionApplied()
    {
        // Nodes with no connections
        List<NetworkNode> nodes = CreateTestNodes(3);
        nodes[0].position = new Vector2(0, 0);
        nodes[1].position = new Vector2(1, 0);
        nodes[2].position = new Vector2(0, 1);

        layout.RunLayout(nodes, 5f);

        // Calculate both forces
        var repelMethod = typeof(ForceDirectedLayout).GetMethod("CalculateRepulsiveForces",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var attractMethod = typeof(ForceDirectedLayout).GetMethod("CalculateAttractiveForces",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        repelMethod.Invoke(layout, null);

        Vector2 velocityAfterRepulsion0 = nodes[0].velocity;
        Vector2 velocityAfterRepulsion1 = nodes[1].velocity;

        attractMethod.Invoke(layout, null);

        // Repulsion should add velocity
        Assert.AreNotEqual(Vector2.zero, velocityAfterRepulsion0,
            "Nodes should repel even without connections");

        // Attraction should not change velocity (no connections)
        Assert.AreEqual(velocityAfterRepulsion0, nodes[0].velocity,
            "Attraction should not affect disconnected nodes");
        Assert.AreEqual(velocityAfterRepulsion1, nodes[1].velocity,
            "Attraction should not affect disconnected nodes");
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
