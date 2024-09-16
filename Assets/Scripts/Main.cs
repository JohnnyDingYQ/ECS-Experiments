// using UnityEngine;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Collections;

// public partial struct Main : ISystem, ISystemStartStop
// {
//     readonly EntityManager entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
//     Curve curve0;
//     Curve curve1;
//     void OnCreate(ref SystemState state)
//     {
//         curve0 = CurveFactory(new(0, 0, 0), new(0, 0, 1), new(0, 0, 2), new(0, 0, 3), ref state);
//         curve1 = CurveFactory(new(0, 0, 3), new(0, 0, 4), new(0, 0, 5), new(0, 0, 6), ref state);
        
//     }

//     public void OnStartRunning(ref SystemState state)
//     {
//         foreach (var curve in SystemAPI.Query<Curve>())
//         {
//             curve.Add(curve1);
//         }

//     }

//     void OnUpdate(ref SystemState state)
//     {
//         // ComponentL
//         // entityManager.GetC
//         // curve1 = CurveFactory(new(0, 0, 3), new(0, 0, 4), new(0, 0, 5), new(0, 0, 6));
//     }


//     public void OnStopRunning(ref SystemState state)
//     {
//     }

//     void OnDestroy(ref SystemState state)
//     {
//         entityManager.DestroyEntity(state.GetEntityQuery(typeof(CurveData)));
//     }

//     Curve CurveFactory(float3 p0, float3 p1, float3 p2, float3 p3, ref SystemState state)
//     {
//         Entity curve = entityManager.CreateEntity(typeof(BeizerCurve), typeof(CurveData));
//         entityManager.SetComponentData(curve, new BeizerCurve()
//         {
//             p0 = p0,
//             p1 = p1,
//             p2 = p2,
//             p3 = p3
//         });
//         entityManager.SetComponentData(curve, new CurveData()
//         {
//             startDistance = 0,
//             endDistance = 0,
//             offsetDistance = 0,
//             startT = 0,
//             endT = 1,
//             root = curve
//         });
//         DynamicBuffer<float3> pointBuffer = entityManager.AddBuffer<DistanceToInterpolationTable>(curve).Reinterpret<float3>();
//         pointBuffer.Add(1);
//         return SystemAPI.GetAspect<Curve>(curve);
//     }

// }
