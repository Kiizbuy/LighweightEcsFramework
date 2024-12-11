namespace CodeGenerator.TestDatas.NestedComponents.FirstCase
{
    public struct HealthComponent : IComponentData
    {
        [FloatMinMaxValue(-100, 100.001f, 0.01f)]
        public float Min;
        [MinMaxValue(-12, 12)]
        public int Max;
        [IgnoreSerialization]
        public int Amount;
    }

    public struct SpeedComponent : IComponentData
    {
        public byte SpeedId;
        public ushort SpeedGen;
        public int TargetId;
    }

    public struct MoveComponentTag : IComponentData
    {
    }
}