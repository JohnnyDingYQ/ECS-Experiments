using System.Linq;
using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class CurveTest
{
    float3 stride = Constants.MinLaneLength * new float3(1, 0, 0);

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
    public void AttributeAccessible()
    {
        Curve curve = Factory.CreateCurve(0, stride, 3 * stride, World.DefaultGameObjectInjectionWorld.EntityManager);
        Assert.AreEqual(0, curve.StartT);
    }

    [Test]
    public void DistanceToInterpolation()
    {
        Curve curve = Factory.CreateCurve(0, stride, 2 * stride, World.DefaultGameObjectInjectionWorld.EntityManager);
        float t = curve.DistanceToInterpolation(math.length(stride));

        Assert.True(MyNumerics.IsApproxEqual(t, 0.5f), $"Expected: 0.5f, Actual: {t}");
    }

    [Test]
    public void MulitpleAddDistance()
    {
        Curve curve = Factory.CreateCurve(0, stride, 3 * stride, World.DefaultGameObjectInjectionWorld.EntityManager);
        float decrement = curve.Length / 5;

        curve = curve.AddStartDistance(decrement);
        float3 prevStart = curve.StartPos;
        curve = curve.AddStartDistance(decrement);
        Assert.AreNotEqual(prevStart, curve.StartPos);
        Assert.True(MyNumerics.IsApproxEqual(math.length(prevStart - curve.StartPos), decrement),
            $"Expected: {math.length(prevStart - curve.StartPos)}, Actual: {decrement}");

        curve.AddEndDistance(decrement);
        float3 prevEnd = curve.EndPos;
        curve.AddEndDistance(decrement);
        Assert.AreNotEqual(prevStart, curve.EndPos);
        Assert.True(MyNumerics.IsApproxEqual(math.length(prevEnd - curve.EndPos), decrement));
    }

    [Test]
    public void EvaluatePosition()
    {
        float longLength = 100;
        float3 up = new(0, 0, 1);
        Curve curve = Factory.CreateCurve(0, up * longLength / 2, up * longLength, World.DefaultGameObjectInjectionWorld.EntityManager);
        float3 evaluated = curve.EvaluatePosition(1);

        Assert.IsTrue(MyNumerics.IsApproxEqual(1 * up, evaluated), $"Expected {1 * up}, Acutal: {evaluated}");
    }

    [Test]
    public void GetNearestDistanceLongCurve()
    {
        float longLength = 2000;
        float3 up = new(0, 0, 1);
        Curve curve = Factory.CreateCurve(0, up * longLength / 2, up * longLength, World.DefaultGameObjectInjectionWorld.EntityManager);
        curve.GetNearestDistance(new Ray(up + new float3(0, 1, 0), new(0, -1, 0)), out float distanceOnCurve);

        Assert.IsTrue(MyNumerics.IsApproxEqual(1, distanceOnCurve), $"Expected {1}, Acutal: {distanceOnCurve}");
    }

    [Test]
    public void ReverseTest()
    {
        float3 up = new(0, 0, 500);
        float3 right = new(500, 0, 0);
        Curve curve = Factory.CreateCurve(0, up, up + right, World.DefaultGameObjectInjectionWorld.EntityManager);
        Curve reversed = curve.Offset(1);
        reversed.Reverse();

        Assert.AreEqual(curve.StartPos + curve.StartNormal, reversed.EndPos);
        Assert.AreEqual(curve.EndPos + curve.EndNormal, reversed.StartPos);
    }

    [Test]
    public void GetInterpolationAtEnd()
    {
        for (float length = 100; length < 2000; length += 50)
        {
            Curve curve = Factory.CreateCurve(0, MyNumerics.Right * length / 2, MyNumerics.Right * length, World.DefaultGameObjectInjectionWorld.EntityManager);
            float t0 = curve.DistanceToInterpolation(length - 1);
            float t1 = curve.DistanceToInterpolation(length - 2);

            Assert.AreNotEqual(t0, t1);
        }
    }

    [Test]
    public void GetNearestDistanceAtEnd()
    {
        for (float length = 100; length < 2000; length += 50)
        {
            Curve curve = Factory.CreateCurve(0, MyNumerics.Right * length / 2, MyNumerics.Right * length, World.DefaultGameObjectInjectionWorld.EntityManager);
            curve.GetNearestDistance(new(new(length - 1, -1, 1), new(0, 1, 0)), out float distA);
            curve.GetNearestDistance(new(new(length - 3, -1, 1), new(0, 1, 0)), out float distB);

            Assert.AreNotEqual(distA, distB);
        }
    }
}