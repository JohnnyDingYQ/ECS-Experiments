using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct EntityStore : IComponentData
{
    public Entity entity;
}

public static class Factory
{
    public const int CurveLutSize = 16;
    public static Curve CreateCurve(float3 p0, float3 p1, float3 p2, EntityManager entityManager)
    {
        EntityCommandBuffer ecb = new(Allocator.Temp);
        Entity entity = ecb.CreateEntity();
        var lut = ecb.AddBuffer<DistanceToInterpolationPair>(entity);
        lut.ResizeUninitialized(CurveLutSize);

        BeizerCurve beizerCurve = new()
        {
            p0 = p0,
            p1 = p0 + 2 / 3 * (p1 - p0),
            p2 = p2 + 2 / 3 * (p1 - p2),
            p3 = p2
        };

        float3 prevPos = EvaluatePosition(beizerCurve, 0);
        float distance = 0;
        lut[0] = new() { interpolation = 0, distance = 0 };
        for (float i = 1; i < CurveLutSize; i++)
        {
            float interpolation = i / (CurveLutSize - 1);
            float3 pos = EvaluatePosition(beizerCurve, interpolation);
            distance += math.length(pos - prevPos);
            lut[(int)i] = new() { interpolation = interpolation, distance = distance };
            // Debug.Log($"t: {interpolation}, distance: {distance}");
            prevPos = pos;
        }
        beizerCurve.length = distance;

        ecb.AddComponent(entity, beizerCurve);
        ecb.AddComponent(entity, new CurveData()
        {
            endT = 1
        });

        Entity store = entityManager.CreateEntity();
        ecb.AddComponent(store, new EntityStore() {entity = entity});

        ecb.Playback(entityManager);
        ecb.Dispose();

        return entityManager.GetAspect<Curve>(entityManager.GetComponentData<EntityStore>(store).entity);

        static float3 EvaluatePosition(BeizerCurve beizerCurve, float t)
        {
            return beizerCurve.p0 * math.pow(1 - t, 3)
                + 3 * math.pow(1 - t, 2) * t * beizerCurve.p1
                + (1 - t) * 3 * math.pow(t, 2) * beizerCurve.p2
                + math.pow(t, 3) * beizerCurve.p3;
        }
    }

}