using System;
using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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


[InternalBufferCapacity(Factory.CurveLutSize)]
public struct DistanceToInterpolationPair : IBufferElementData
{
    public float distance;
    public float interpolation;
}

public readonly partial struct Curve : IAspect
{
    public const int CurveLutSize = 16;
    public readonly Entity entity;
    readonly RefRW<CurveData> curveData;
    readonly RefRW<BeizerCurve> beizerCurve;
    readonly DynamicBuffer<DistanceToInterpolationPair> lut;
    // EntityManager entityManager { get => World.DefaultGameObjectInjectionWorld.EntityManager; }

    public float OffsetDistance { get => curveData.ValueRO.offsetDistance; set => curveData.ValueRW.offsetDistance = value; }
    public float StartDistance { get => curveData.ValueRO.startDistance; set => curveData.ValueRW.startDistance = value; }
    public float EndDistance { get => curveData.ValueRO.endDistance; set => curveData.ValueRW.endDistance = value; }
    public float StartT { get => curveData.ValueRO.startT; set => curveData.ValueRW.startT = value; }
    public float EndT { get => curveData.ValueRO.endT; set => curveData.ValueRW.endT = value; }
    public float Length { get => beizerCurve.ValueRO.length - curveData.ValueRO.startDistance - curveData.ValueRO.endDistance; }
    public float BCurveLength { get => beizerCurve.ValueRO.length; }
    public float3 P0 { get => beizerCurve.ValueRO.p0; set => beizerCurve.ValueRW.p0 = value; }
    public float3 P1 { get => beizerCurve.ValueRO.p1; set => beizerCurve.ValueRW.p1 = value; }
    public float3 P2 { get => beizerCurve.ValueRO.p2; set => beizerCurve.ValueRW.p2 = value; }
    public float3 P3 { get => beizerCurve.ValueRO.p3; set => beizerCurve.ValueRW.p3 = value; }

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

    public unsafe void CalculateLut()
    {
        float3 prevPos = P0;
        float distance = 0;
        // lut.GetUnsafePtr

        DistanceToInterpolationPair* ptr = (DistanceToInterpolationPair*)lut.GetUnsafePtr();
        ptr[0] = new() { interpolation = 0, distance = 0 };
        for (float i = 1; i < CurveLutSize; i++)
        {
            float interpolation = i / (CurveLutSize - 1);
            float3 pos = EvaluatePositionT(interpolation);
            distance += math.length(pos - prevPos);
            ptr[(int)i] = new() { interpolation = interpolation, distance = distance };
            // Debug.Log($"t: {interpolation}, distance: {distance}");
            prevPos = pos;
        }
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

    public float DistanceToInterpolation(float distance)
    {
        DistanceToInterpolationPair low = lut[0];
        DistanceToInterpolationPair high = lut[1];
        int index = 2;
        while (high.distance < distance && index < 15)
        {
            low = lut[index - 1];
            high = lut[index];
            index++;
        }

        // Debug.Log($"low: {low.distance}, high: {high.distance}");
        // Debug.Log($"low: {low.interpolation}, high: {high.interpolation}");
        // Debug.Log((distance - low.distance) / (high.distance - low.distance));
        float midT = math.lerp(low.interpolation, high.interpolation, (distance - low.distance) / (high.distance - low.distance));
        float midDistance = low.distance + math.length(EvaluatePositionT(midT) - EvaluatePositionT(low.interpolation));
        DistanceToInterpolationPair mid = new() { distance = midDistance, interpolation = midT };

        int count = 0;
        while (math.length(mid.distance - distance) > 0.01f && count++ < 10)
        {
            if (mid.distance < distance)
                low = mid;
            else
                high = mid;
            mid.interpolation = math.lerp(low.interpolation, high.interpolation, 0.5f);
            mid.distance = low.distance + math.length(EvaluatePositionT(mid.interpolation) - EvaluatePositionT(low.interpolation));
        }

        return mid.interpolation;
    }

    float3 EvaluatePositionT(float t)
    {
        return P0 * math.pow(1 - t, 3)
               + 3 * math.pow(1 - t, 2) * t * P1
               + (1 - t) * 3 * math.pow(t, 2) * P2
               + math.pow(t, 3) * P3;
    }

    float3 EvaluateTangentT(float t)
    {
        return 3 * math.pow(1 - t, 2) * (P1 - P0) + 6 * (1 - t) * t * (P2 - P1) + 3 * math.pow(t, 2) * (P3 - P2);
    }

    float3 EvaluateNormalT(float t)
    {
        float3 tangent = EvaluateTangentT(t);
        float3 normal = new(-tangent.z, 0, tangent.x);
        return math.normalizesafe(normal);
    }

    public float3 EvaluatePosition(float distance)
    {
        float t = DistanceToInterpolation(distance);
        return EvaluatePositionT(t) + OffsetDistance * EvaluateNormalT(t);
    }

    public float3 EvaluateTangent(float distance)
    {
        return math.normalizesafe(EvaluateTangentT(DistanceToInterpolation(distance)));
    }

    public float3 EvaluateNormal(float distance)
    {
        return EvaluateNormalT(DistanceToInterpolation(distance));
    }

    public void Reverse()
    {
        (P0, P3) = (P3, P0);
        (P1, P2) = (P2, P1);
        (StartDistance, EndDistance) = (EndDistance, StartDistance);
        OffsetDistance *= -1;
        StartT = DistanceToInterpolation(StartDistance);
        EndT = DistanceToInterpolation(EndDistance);
    }

    public float GetNearestDistance(Ray ray, out float distanceOnCurve, int resolution = 10)
    {
        float getNearestPointTolerance = 0.001f;
        float minDistance = float.MaxValue;
        distanceOnCurve = 0;
        float distanceStep = Length / resolution;
        float localMin = 0;
        while (distanceOnCurve <= Length)
        {
            float3 pos = EvaluatePosition(distanceOnCurve);
            float distance = GetDistanceToCurve(pos);
            if (distance < minDistance)
            {
                minDistance = distance;
                localMin = distanceOnCurve;
            }
            distanceOnCurve += distanceStep;
        }
        float low = localMin - distanceStep >= 0 ? localMin - distanceStep : 0;
        float high = localMin + distanceStep <= Length ? localMin + distanceStep : Length;
        do
        {
            float mid = (low + high) / 2;
            if (GetDistanceToCurve(EvaluatePosition(Mathf.Max(0, mid - getNearestPointTolerance)))
                < GetDistanceToCurve(EvaluatePosition(Mathf.Min(Length, mid + getNearestPointTolerance))))
                high = mid;
            else
                low = mid;
        } while (high - low > getNearestPointTolerance);

        distanceOnCurve = low;
        return GetDistanceToCurve(EvaluatePosition(low));

        float GetDistanceToCurve(float3 pos)
        {
            return Vector3.Cross(ray.direction, (Vector3)pos - ray.origin).magnitude;
        }
    }
}