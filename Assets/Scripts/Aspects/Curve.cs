using System;
using Unity.Entities;
using Unity.Mathematics;

public struct CurveData : IComponentData
{

    public float startDistance;
    public float offsetDistance;
    public float endDistance;
    public float startT;
    public float endT;
    public Curve nextCurve;
    public LinkedEntityGroup root;
}
public struct BeizerCurve : IComponentData
{
    public float3 P0;
    public float3 P1;
    public float3 P2;
    public float3 P3;
    public float Length;
}


[InternalBufferCapacity(15)]
public struct PointBuffer : IBufferElementData
{
    public float3 Pos;
}

public readonly partial struct Curve : IAspect
{
    public readonly Entity entity;
    readonly RefRW<CurveData> curveData;
    readonly RefRW<BeizerCurve> beizerCurve;
    EntityManager entityManager { get => World.DefaultGameObjectInjectionWorld.EntityManager; }

    public float offsetDistance { get => curveData.ValueRO.offsetDistance; set => curveData.ValueRW.offsetDistance = value; }
    public float startDistance { get => curveData.ValueRO.startDistance; set => curveData.ValueRW.startDistance = value; }
    public float endDistance { get => curveData.ValueRO.endDistance; set => curveData.ValueRW.endDistance = value; }
    public float startT { get => curveData.ValueRO.startT; set => curveData.ValueRW.startT = value; }
    public float endT { get => curveData.ValueRO.endT; set => curveData.ValueRW.endT = value; }
    // public float EndT { get => entityManager.GetComponentData<CurveData>(entity).endT; }
    public float SegmentLength { get => beizerCurve.ValueRO.Length - curveData.ValueRO.startDistance - curveData.ValueRO.endDistance; }
    public Curve nextCurve { get => curveData.ValueRO.nextCurve; set => curveData.ValueRW.nextCurve = value; }
    public Entity root { get => curveData.ValueRO.root.Value; set => curveData.ValueRW.root.Value = value; }

    #region Public Methods

    bool IsNull(Curve curveAspect)
    {
        return curveAspect.Equals(default(Curve));
    }

    /// <summary>
    /// Creates a deep copy of the current curve
    /// </summary>
    /// <returns>Deep copy</returns>
    public Curve Duplicate(Entity? newRoot = null)
    {
        Entity entityDuplication = entityManager.Instantiate(entity);
        Entity root = newRoot == null ? entityDuplication : newRoot.Value;
        Curve duplicated = entityManager.GetAspect<Curve>(entityDuplication);
        duplicated.root = root;
        if (!IsNull(duplicated.nextCurve))
            duplicated.nextCurve = duplicated.nextCurve.Duplicate(root);

        return duplicated;
    }

    /// <summary>
    /// Remove a portion of the curve a given distance from the start
    /// </summary>
    /// <param name="distance">Distance from start</param>
    /// <returns>Truncated Curve</returns>
    /// <exception cref="ArgumentException">Given distance is negative</exception>
    public Curve AddStartDistance(float distance)
    {
        if (distance < 0)
            throw new ArgumentException("distance cannot be negative", "distance");
        Curve curr = this;
        while (curr.SegmentLength < distance)
        {
            distance -= curr.SegmentLength;
            curr = curr.nextCurve;
        }
        curr.startDistance += distance;
        // curr.startT = CurveUtility.GetDistanceToInterpolation(curr.lut, curr.startDistance);
        return curr;
    }

    // /// <summary>
    // /// Remove a portion of the curve a given distance from the end
    // /// </summary>
    // /// <param name="distance">Distance from end</param>
    // /// <returns>Truncated Curve</returns>
    // /// <exception cref="ArgumentException">Given distance is negative</exception>
    // public Curve AddEndDistance(float distance)
    // {
    //     if (distance < 0)
    //         throw new ArgumentException("distance cannot be negative", "distance");
    //     int index = GetChainLength() - 1;
    //     Curve curr = GetCurveByIndex(index);
    //     while (curr.SegmentLength < distance)
    //     {
    //         distance -= curr.SegmentLength;
    //         curr = GetCurveByIndex(--index);
    //         curr.nextCurve = null;
    //     }
    //     curr.endDistance += distance;
    //     curr.endT = CurveUtility.GetDistanceToInterpolation(curr.lut, curr.bCurveLength - curr.endDistance);
    //     return this;
    // }

    // /// <summary>
    // /// Reverse the direction of the curve
    // /// </summary>
    // /// <returns>The reversed curve</returns>
    // public Curve Reverse()
    // {
    //     Curve newHead = ReverseLinkedList(this);

    //     newHead = ReverseHelper(newHead);
    //     return newHead;

    //     static Curve ReverseLinkedList(Curve head)
    //     {
    //         if (head == null || head.nextCurve == null)
    //             return head;
    //         Curve prev = null;
    //         Curve curr = head;
    //         while (curr != null)
    //         {
    //             Curve next = curr.nextCurve;
    //             curr.nextCurve = prev;
    //             prev = curr;
    //             curr = next;
    //         }

    //         return prev;
    //     }

    //     static Curve ReverseHelper(Curve curve)
    //     {
    //         if (curve == null)
    //             return null;
    //         Curve reversed = new()
    //         {
    //             bCurve = curve.bCurve.GetInvertedCurve(),
    //             startDistance = curve.endDistance,
    //             endDistance = curve.startDistance,
    //             bCurveLength = curve.bCurveLength,
    //             offsetDistance = -curve.offsetDistance,
    //             nextCurve = curve.nextCurve
    //         };
    //         reversed.nextCurve = ReverseHelper(reversed.nextCurve);
    //         reversed.CreateDistanceCache();
    //         reversed.startT = reversed.GetDistanceToInterpolation(curve.startDistance);
    //         reversed.endT = reversed.GetDistanceToInterpolation(curve.bCurveLength - curve.endDistance);
    //         return reversed;
    //     }
    // }

    // /// <summary>
    // /// Offset the curve a given distance with respect to its 2D normal
    // /// </summary>
    // /// <param name="distance">Offset distance</param>
    // /// <returns>Offsetted Curve</returns>
    // public Curve Offset(float distance)
    // {
    //     Curve curve = this;
    //     while (curve != null)
    //     {
    //         curve.offsetDistance += distance;
    //         curve = curve.nextCurve;
    //     }
    //     return this;
    // }

    // /// <summary>
    // /// Get an IEnumerable of equally distanced points on the curve
    // /// </summary>
    // /// <param name="numPoints"></param>
    // /// <returns></returns>
    // public OutlineEnum GetOutline(int numPoints)
    // {
    //     return new(this, numPoints);
    // }

    /// <summary>
    /// Add the start of another curve to the end of this curve
    /// </summary>
    /// <param name="other">The other curve</param>
    public void Add(Curve other)
    {
        Curve last = GetLastCurve();
        // last.nextCurve = other.Duplicate(entity);
        last.nextCurve = other;
    }

    Curve GetLastCurve()
    {
        float3 float3 = startDistance;
        if (IsNull(nextCurve))
            return this;
        return nextCurve.GetLastCurve();
    }

    // /// <summary>
    // /// Evaluate the position of a given distance on curve 
    // /// </summary>
    // /// <param name="distance">The given distance</param>
    // /// <returns>Position on curve</returns>
    // /// <exception cref="ArgumentException">Given distance is negative</exception>
    // public float3 EvaluatePosition(float distance)
    // {
    //     if (distance < 0)
    //         throw new ArgumentException("distance cannot be negative", "distance");
    //     if (distance > SegmentLength && nextCurve != null)
    //         return nextCurve.EvaluatePosition(distance - SegmentLength);
    //     float t = CurveUtility.GetDistanceToInterpolation(lut, startDistance + distance);
    //     if (offsetDistance == 0)
    //         return CurveUtility.EvaluatePosition(bCurve, t);
    //     return CurveUtility.EvaluatePosition(bCurve, t) + Normalized2DNormal(bCurve, t) * offsetDistance;
    // }

    // /// <summary>
    // /// Evaluate the tangent of a given distance on curve
    // /// </summary>
    // /// <param name="distance">The given distance</param>
    // /// <returns>Tangent</returns>
    // /// <exception cref="ArgumentException">Given distance is negative</exception>
    // public float3 EvaluateTangent(float distance)
    // {
    //     if (distance < 0)
    //         throw new ArgumentException("distance cannot be negative", "distance");
    //     if (distance > SegmentLength && nextCurve != null)
    //         return nextCurve.EvaluateTangent(distance - SegmentLength);
    //     float t = CurveUtility.GetDistanceToInterpolation(lut, startDistance + distance);
    //     return math.normalize(CurveUtility.EvaluateTangent(bCurve, t));
    // }

    // /// <summary>
    // /// Evaluate the normalized 2D (xz plane) normal of a given distance on curve
    // /// </summary>
    // /// <param name="distance">The given distance</param>
    // /// <returns>Normalized 2D nomral</returns>
    // /// <exception cref="ArgumentException">Given distance is negative</exception>
    // public float3 Evaluate2DNormal(float distance)
    // {
    //     if (distance < 0)
    //         throw new ArgumentException("distance cannot be negative", "distance");
    //     if (distance > SegmentLength && nextCurve != null)
    //         return nextCurve.Evaluate2DNormal(distance - SegmentLength);
    //     float t = CurveUtility.GetDistanceToInterpolation(lut, startDistance + distance);
    //     return math.normalize(Normalized2DNormal(bCurve, t));
    // }

    // /// <summary>
    // /// Compute the minimum distance between the curve and a given ray
    // /// </summary>
    // /// <param name="ray">The given ray</param>
    // /// <param name="distanceOnCurve">The distance on curve when the said minimum occurs</param>
    // /// <param name="resolution">Higher resolution prevents local minimum from being mistaken as global minimum</param>
    // /// <returns>The minimum distance between the curve and a given ray</returns>
    // public float GetNearestDistance(Ray ray, out float distanceOnCurve, int resolution = 10)
    // {
    //     float minDistance = float.MaxValue;
    //     distanceOnCurve = 0;
    //     float distanceStep = Length / resolution;
    //     float localMin = 0;
    //     while (distanceOnCurve <= Length)
    //     {
    //         float3 pos = EvaluatePosition(distanceOnCurve);
    //         float distance = GetDistanceToCurve(pos);
    //         if (distance < minDistance)
    //         {
    //             minDistance = distance;
    //             localMin = distanceOnCurve;
    //         }
    //         distanceOnCurve += distanceStep;
    //     }
    //     float low = localMin - distanceStep >= 0 ? localMin - distanceStep : 0;
    //     float high = localMin + distanceStep <= Length ? localMin + distanceStep : Length;
    //     do
    //     {
    //         float mid = (low + high) / 2;
    //         if (GetDistanceToCurve(EvaluatePosition(Mathf.Max(0, mid - getNearestPointTolerance)))
    //             < GetDistanceToCurve(EvaluatePosition(Mathf.Min(Length, mid + getNearestPointTolerance))))
    //             high = mid;
    //         else
    //             low = mid;
    //     } while (high - low > getNearestPointTolerance);

    //     distanceOnCurve = low;
    //     return GetDistanceToCurve(EvaluatePosition(low));

    //     float GetDistanceToCurve(float3 pos)
    //     {
    //         return Vector3.Cross(ray.direction, (Vector3)pos - ray.origin).magnitude;
    //     }
    // }

    // /// <summary>
    // /// Split the curve with a given distance on curve
    // /// </summary>
    // /// <param name="distance">The given distance</param>
    // /// <param name="left">Left curve</param>
    // /// <param name="right">Right curve</param>
    // /// <exception cref="ArgumentException">Split distance is greater than curve length</exception>
    // public void Split(float distance, out Curve left, out Curve right)
    // {
    //     if (distance >= Length)
    //         throw new ArgumentException("distance has to be smaller than curve length", "distance");
    //     int index = 0;
    //     Curve toSplit = this;
    //     float currDistance = distance;

    //     while (true)
    //     {
    //         if (Math.Abs(currDistance - toSplit.SegmentLength) < minimumCurveLength)
    //         {
    //             left = this;
    //             right = toSplit.nextCurve;
    //             toSplit.nextCurve = null;
    //             return;
    //         }
    //         if (currDistance >= toSplit.SegmentLength)
    //         {
    //             currDistance -= toSplit.SegmentLength;
    //             toSplit = toSplit.nextCurve;
    //             index++;
    //         }
    //         else
    //             break;
    //     }

    //     CurveUtility.Split(toSplit.bCurve, toSplit.GetDistanceToInterpolation(currDistance), out BezierCurve l, out BezierCurve r);
    //     left = new(l) { offsetDistance = offsetDistance };
    //     left = left.AddStartDistance(startDistance);
    //     right = new(r) { offsetDistance = offsetDistance };
    //     right = right.AddEndDistance(endDistance);

    //     if (index == 0)
    //     {
    //         right.nextCurve = nextCurve;
    //         return;
    //     }
    //     Curve newHead = Duplicate();
    //     Curve prev = newHead;
    //     while (index != 1)
    //     {
    //         prev = prev.nextCurve;
    //         index--;
    //     }
    //     Curve next = prev.nextCurve.nextCurve;
    //     prev.nextCurve = left;
    //     left = newHead;
    //     right.nextCurve = next;
    // }

    // public float3 LerpPosition(float distance)
    // {
    //     if (distance >= Length)
    //         return segmentCache[^1];
    //     if (segmentCache == null)
    //         InitSegmentCache();
    //     float segmentLength = Length / (segmentCache.Length - 1);
    //     int index = (int)(distance / segmentLength);
    //     if (index == segmentCache.Length - 1)
    //         index--;
    //     Assert.IsTrue(segmentLength != 0);
    //     if (index + 1 >= segmentCache.Length)
    //         index += 0;
    //     return math.lerp(segmentCache[index], segmentCache[index + 1], (distance - index * segmentLength) / segmentLength);
    // }

    // public Curve GetNextCurve()
    // {
    //     return nextCurve;
    // }

    // public void CreateDistanceCache()
    // {
    //     lut = new DistanceToInterpolation[distanceToInterpolationCacheSize];
    //     CurveUtility.CalculateCurveLengths(bCurve, lut);
    // }

    #endregion

}