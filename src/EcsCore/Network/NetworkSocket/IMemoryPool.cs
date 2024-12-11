namespace EcsCore.Serialization
{
    public interface IMemoryPool
    {
        byte[] Rent(int minimumSize);
        void Release(byte[] chunk);
    }
}