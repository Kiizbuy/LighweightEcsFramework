using EcsCore;
using EcsCore.Components;
using EcsCore.Network;
using EcsCore.Serialization;

namespace EcsTests
{
    public struct ExcludeComponent : IComponentData, ISerializableData
    {
        private bool _disabled;
        public void Serialize(ISerializePacker serializePacker)
        {
        }

        public void Deserialize(ISerializePacker serializePacker)
        {
        }
    }
    
    public struct TestComponent : IComponentData, ISerializableData
    {
        public int TestValue;

        public void Serialize(ISerializePacker serializePacker)
        {
            serializePacker.Write(TestValue);
        }

        public void Deserialize(ISerializePacker serializePacker)
        {
            TestValue = serializePacker.ReadInt();
        }
    }

    public struct TestComponentData : IComponentData, ISerializableData
    {
        public int TestValue;

        public void Serialize(ISerializePacker serializePacker)
        {
            serializePacker.Write(TestValue);
        }

        public void Deserialize(ISerializePacker serializePacker)
        {
            TestValue = serializePacker.ReadInt();
        }
    }

    public struct ExcludeComponentData : IComponentData, ISerializableData
    {
        public void Serialize(ISerializePacker serializePacker)
        {
        }

        public void Deserialize(ISerializePacker serializePacker)
        {
        }
    }

    public struct TestSerializeDiffData : IDiffableSerializableData<TestSerializeDiffData>
    {
        public int Velocity;
        public int Min;
        public int Max;

        public void Serialize(ISerializePacker serializePacker)
        {
            serializePacker.Write(Velocity);
            serializePacker.Write(Min);
            serializePacker.Write(Max);
        }

        public void Deserialize(ISerializePacker serializePacker)
        {
            Velocity = serializePacker.ReadInt();
            Min = serializePacker.ReadInt();
            Max = serializePacker.ReadInt();
        }

        public void SerializeDiffable(ISerializePacker serializePacker, TestSerializeDiffData other)
        {
            var diffableData = other;

            serializePacker.Write(Velocity != diffableData.Velocity);
            if (Velocity != diffableData.Velocity)
                serializePacker.Write(Velocity);

            serializePacker.Write(Min != diffableData.Min);
            if (Min != diffableData.Min)
                serializePacker.Write(Min);

            serializePacker.Write(Max != diffableData.Max);
            if (Max != diffableData.Max)
                serializePacker.Write(Max);
        }

        public void DeserializeDiffable(ISerializePacker serializePacker, TestSerializeDiffData other)
        {
            var diffableData = other;

            Velocity = serializePacker.ReadBool() ? serializePacker.ReadInt() : diffableData.Velocity;
            Min = serializePacker.ReadBool() ? serializePacker.ReadInt() : diffableData.Min;
            Max = serializePacker.ReadBool() ? serializePacker.ReadInt() : diffableData.Max;

        }
    }
}