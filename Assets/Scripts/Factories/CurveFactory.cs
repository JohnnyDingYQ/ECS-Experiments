using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public static partial class Factory
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

        ecb.AddComponent(entity, beizerCurve);
        ecb.AddComponent(entity, new CurveData()
        {
            endT = 1
        });

        Entity store = entityManager.CreateEntity();
        ecb.AddComponent(store, new EntityStore() {entity = entity});

        ecb.Playback(entityManager);
        ecb.Dispose();

        Curve curve = entityManager.GetAspect<Curve>(entityManager.GetComponentData<EntityStore>(store).entity);
        curve.CalculateLut();
        return curve;
    }

}