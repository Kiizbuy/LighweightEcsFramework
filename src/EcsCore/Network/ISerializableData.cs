namespace EcsCore.Serialization
{
    public interface ISerializableData
    {
        void Serialize(ISerializePacker serializePacker);
        void Deserialize(ISerializePacker serializePacker);
    }
    
    public interface IDiffableSerializableData<in T> : ISerializableData
    {
        void SerializeDiffable(ISerializePacker serializePacker, T other);
        void DeserializeDiffable(ISerializePacker serializePacker, T other);
    }
}