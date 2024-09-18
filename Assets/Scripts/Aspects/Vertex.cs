using System;
using System.Linq;
using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct VertexData : IComponentData
{
    public float3 pos;
}

public struct InEdges : IBufferElementData
{
    public Vertex source;
}

public struct OutEdges : IBufferElementData
{
    public Vertex target;
}

public readonly partial struct Vertex : IAspect, IBufferElementData
{
    readonly DynamicBuffer<InEdges> InEdges;
    readonly DynamicBuffer<OutEdges> OutEdges;

    public NativeArray<Vertex> GetOutVertices()
    {
        var temp = OutEdges.ToNativeArray(Allocator.Temp);
        var ans = new NativeArray<Vertex>(temp.Length, Allocator.Temp);
        for (int i = 0; i < temp.Length; i++)
            ans[i] = temp[i].target;
        temp.Dispose();
        return ans;
    }

    public void AddOutVertex(Vertex vertex)
    {
        var outVertices = GetOutVertices();
        foreach (var v in outVertices)
            if (Equals(v, vertex))
                return;
        OutEdges.Add(new() { target = vertex });
        outVertices.Dispose();
    }
}