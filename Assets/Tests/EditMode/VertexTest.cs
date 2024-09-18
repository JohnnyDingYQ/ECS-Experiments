using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class VertexTest
{
    [SetUp]
    public void SetUp()
    {
        World.DisposeAllWorlds();
        World.DefaultGameObjectInjectionWorld = new("testing");
    }

    [TearDown]
    public void TearDown()
    {
        World.DisposeAllWorlds();
        World.DefaultGameObjectInjectionWorld = null;
    }

    [Test]
    public void VertexOutEdges()
    {
        Graph graph = Factory.CreateGraph(World.DefaultGameObjectInjectionWorld.EntityManager);
    }
}