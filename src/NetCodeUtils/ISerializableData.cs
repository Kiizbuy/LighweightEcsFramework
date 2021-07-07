namespace NetCodeUtils
{
    public interface ISerializableData
    {
        void Serialize(IPacker packer);
        void Deserialize(IPacker packer);
    }

    public interface IDiffableSerializableData<in T> : ISerializableData
    {
        void SerializeDiffable(IPacker packer, T other);
        void DeserializeDiffable(IPacker packer, T other);
    }
}