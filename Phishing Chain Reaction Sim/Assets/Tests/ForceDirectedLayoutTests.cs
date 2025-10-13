
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

    private List<NetworkNode> CreateTestNodes(int nodeCount)
    {
        List<NetworkNode> nodes = new List<NetworkNode>();

        // reused from GraphManager.GenerateGraph()
        for (int i = 0; i < nodeCount; i++)
        {
            NetworkNode node = new NetworkNode(i);

            // nodes are initialised on the circumference
            float angle = i / (float)nodeCount * Mathf.PI * 2; // evenly space nodes
            node.position = new Vector2(                        // convert polar to cartesian coordinates
                Mathf.Cos(angle) * 5f,  // constant graph radius
                Mathf.Sin(angle) * 5f
            );

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
