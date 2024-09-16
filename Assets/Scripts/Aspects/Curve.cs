using System;
using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;

public struct CurveData : IComponentData
{
    public float startDistance;
    public float offsetDistance;
    public float endDistance;
    public float startT;
    public float endT;
}
public struct BeizerCurve : IComponentData
{
    public float3 p0;
    public float3 p1;
    public float3 p2;
    public float3 p3;
    public float length;
}


[InternalBufferCapacity(15)]
public struct DistanceToInterpolationPair : IBufferElementData
{
    public float distance;
    public float interpolation;
}

public readonly partial struct Curve : IAspect
{
    public readonly Entity entity;
    readonly RefRW<CurveData> curveData;
    readonly RefRO<BeizerCurve> beizerCurve;
    readonly DynamicBuffer<DistanceToInterpolationPair> lut;
    // EntityManager entityManager { get => World.DefaultGameObjectInjectionWorld.EntityManager; }

    public float OffsetDistance { get => curveData.ValueRO.offsetDistance; set => curveData.ValueRW.offsetDistance = value; }
    public float StartDistance { get => curveData.ValueRO.startDistance; set => curveData.ValueRW.startDistance = value; }
    public float EndDistance { get => curveData.ValueRO.endDistance; set => curveData.ValueRW.endDistance = value; }
    public float StartT { get => curveData.ValueRO.startT; set => curveData.ValueRW.startT = value; }
    public float EndT { get => curveData.ValueRO.endT; set => curveData.ValueRW.endT = value; }
    public float Length { get => beizerCurve.ValueRO.length - curveData.ValueRO.startDistance - curveData.ValueRO.endDistance; }
    public float BCurveLength { get => beizerCurve.ValueRO.length; }
    public float3 P0 { get => beizerCurve.ValueRO.p0; }
    public float3 P1 { get => beizerCurve.ValueRO.p1; }
    public float3 P2 { get => beizerCurve.ValueRO.p2; }
    public float3 P3 { get => beizerCurve.ValueRO.p3; }

    public float3 StartPos { get => EvaluatePosition(StartT); }
    public float3 EndPos { get => EvaluatePosition(EndT); }

    public void Add(Curve other)
    {
        if (!P0.Equals(other.P0) || !P1.Equals(other.P1)
            || !P2.Equals(other.P2) || !P3.Equals(other.P3))
            throw new ArgumentException("given curve have different control points", "other");
        float temp = Length;
        AddEndDistance(-other.Length);
        other.AddStartDistance(-temp);
    }

    public Curve AddStartDistance(float distance)
    {
        StartDistance += distance;
        Assert.IsTrue(StartDistance >= 0 && StartDistance <= BCurveLength,
            $"startDistance: {StartDistance}, bCurveLength: {BCurveLength}");
        StartT = DistanceToInterpolation(StartDistance);
        return this;
    }

    public Curve AddEndDistance(float distance)
    {
        EndDistance += distance;
        Assert.IsTrue(EndDistance >= 0 && EndDistance <= BCurveLength,
            $"endDistance: {EndDistance}, bCurveLength: {BCurveLength}");
        EndT = DistanceToInterpolation(BCurveLength - EndDistance);
        return this;
    }

    float DistanceToInterpolation(float distance)
    {
        DistanceToInterpolationPair low = lut[0];
        DistanceToInterpolationPair high = lut[1];
        int index = 2;
        while (distance > high.distance && index < 15)
        {
            low = lut[index - 1];
            high = lut[index];
            index++;
        }

        return math.lerp(low.interpolation, high.interpolation, (distance - low.distance) / (high.distance - low.distance));
    }

    public float3 EvaluatePosition(float t)
    {
        return P0 * math.pow(1 - t, 3)
               + 3 * math.pow(1 - t, 2) * t * P1
               + (1 - t) * 3 * math.pow(t, 2) * P2
               + math.pow(t, 3) * P3 + OffsetDistance * EvaluateNormal(t);
    }

    public float3 EvaluateTangent(float t)
    {
        return 3 * math.pow(1 - t, 2) * (P1 - P0) + 6 * (1 - t) * t * (P2 - P1) + 3 * math.pow(t, 2) * (P3 - P2);
    }

    public float3 EvaluateNormal(float t)
    {
        float3 tangent = EvaluateTangent(t);
        float3 normal = new(-tangent.z, 0, tangent.x);
        return math.normalizesafe(normal);
    }
}