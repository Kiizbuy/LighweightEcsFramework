using EcsCore.Components;
using NetCodeUtils;

namespace EcsTests
{
    public struct TestComponent : IComponentData, ISerializableData
    {
        public int TestValue;
        public void Serialize(IPacker packer)
        {
            packer.WriteInt(TestValue);
        }

        public void Deserialize(IPacker packer)
        {
            TestValue = packer.ReadInt();
        }
    }

    public struct TestSerializeDiffData : IDiffableSerializableData<TestSerializeDiffData>
    {
        public int Velocity;
        public int Min;
        public int Max;
       
        public void Serialize(IPacker packer)
        {
            packer.WriteFloat(Velocity);
            packer.WriteInt(Min);
            packer.WriteInt(Max);
        }

        public void Deserialize(IPacker packer)
        {
            Velocity = packer.ReadInt();
            Min = packer.ReadInt();
            Max = packer.ReadInt();
        }

        public void SerializeDiffable(IPacker packer, TestSerializeDiffData other)
        {
            var diffableData = other;
            
            packer.WriteBool(Velocity != diffableData.Velocity);
            if(Velocity != diffableData.Velocity)
                packer.WriteInt(Velocity);
            
            packer.WriteBool(Min != diffableData.Min);
            if(Min != diffableData.Min)
                packer.WriteInt(Min);

            packer.WriteBool(Max != diffableData.Max);
            if(Max != diffableData.Max)
                packer.WriteInt(Max);
        }

        public void DeserializeDiffable(IPacker packer, TestSerializeDiffData other)
        {
            var diffableData = other;

            Velocity = packer.ReadBool() ? packer.ReadInt() : diffableData.Velocity;
            Min = packer.ReadBool() ? packer.ReadInt() : diffableData.Min;
            Max = packer.ReadBool() ? packer.ReadInt() : diffableData.Max;

        }
    }
}