using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class GraphTest
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
    public void GraphCreation()
    {
        Graph graph = Factory.CreateGraph(World.DefaultGameObjectInjectionWorld.EntityManager);
    }
}