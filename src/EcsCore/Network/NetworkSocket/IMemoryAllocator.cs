namespace EcsCore.Serialization
{
    public interface IMemoryAllocator
    {
        byte[] Allocate(int size);
    }
}