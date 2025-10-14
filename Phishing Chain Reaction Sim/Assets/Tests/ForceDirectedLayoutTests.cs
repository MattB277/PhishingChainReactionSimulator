
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
        Assert.AreEqual(nodes[0].velocity, nodes[1].velocity, "ForceDirectedLayout.CalculateRepulsiveForces(): Node velocities should be equal and opposite");
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
