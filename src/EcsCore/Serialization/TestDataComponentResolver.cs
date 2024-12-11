using Components;

namespace EcsCore.Serialization.Resolvers
{
    public sealed class TestDataComponentResolver : ComponentResolver<TestDataComponentData, TestDataComponentResolver.Data>
    {
        public struct Data : ISerializableData
        {
            public int Id;
            public float Speed;

            public void Serialize(ISerializePacker serializePacker)
            {
                serializePacker.Write(Id);
                serializePacker.Write(Speed);
            }

            public void Deserialize(ISerializePacker serializePacker)
            {
                Id = serializePacker.ReadInt();
                Speed = serializePacker.ReadFloat();
            }
        }

        protected override Data FillSerializableDataFrom(ref TestDataComponentData componentData)
        {
            Data data;
            data.Id = componentData.Id;
            data.Speed = componentData.speed;
            return data;
        }

        protected override void FillComponent(ref Data data, ref TestDataComponentData componentData)
        {
            componentData.speed = data.Speed;
            componentData.Id = data.Id;
        }
    }
}