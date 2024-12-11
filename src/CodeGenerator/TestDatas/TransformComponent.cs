using System;

namespace CodeGenerator.TestDatas
{
    [FixedArrayGeneration(4)]
    public partial struct PlayerDataFixedArray : IFixedArray<uint>
    {
    }

    public struct TransformComponent : IComponentData
    {
        public PlayerDataFixedArray FixedArray;
        [IgnoreSerialization]
        public float X;
        [IgnoreSerialization]
        public float Y;
        public float Z;

        public string Name;
    }

    public struct Zalupa
    {
    }
}