using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

// public struct Vertices : IBufferElementData
// {
//     public Entity source;
// }

public readonly partial struct Graph : IAspect
{
    readonly DynamicBuffer<Vertex> Vertices;


}