public static class Constants
{
    public const float LaneWidth = 2f;
    public const float BuildSnapTolerance = LaneWidth / 2.5f;
    public const float MinLaneLength = LaneWidth * 5;
    public const float MaxRoadCurveLength = 200;
    public const int MaxLaneCount = 3;
    public const float RoadOutlineSeparation = LaneWidth / 2;
    public const float VertexDistanceFromRoadEnds = MinLaneLength / 2.3f; 
    public const float MaxRoadBendAngle = 130;
    public const int MaxElevation = 20;
    public const int MinElevation = 0;
    public const int ElevationStep = 4;
    public const float CarAcceleration = 35;
    public const float CarDeceleration = 35f;
    public const float CarMaxSpeed = 50f;
    public const float CarMinSpeed = 0f;
    public const int MaxCarCount = 5000;
    public const float CarMinimumSeparation = MinLaneLength / 1.2f;
    public const float DefaultParallelSpacing = LaneWidth * 4.5f;
    public const float MaxRampGrade = 10;
    public const float EdgeCostIncreaseForPath = 0.1f;
}