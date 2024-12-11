
using EcsCore.Components;
using EcsCore.Serialization;

namespace Components
{
    public struct TestDataComponentData : ISerializableComponentData
    {
        [MinMaxValue(-100,100)]
        public int Id;
        [FloatMinMaxValue(-25.5f, 25.5f, 0.001f)]
        public float speed;
    }
}