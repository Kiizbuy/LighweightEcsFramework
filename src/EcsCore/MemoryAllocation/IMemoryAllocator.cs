namespace EcsCore.MemoryAllocation
{
    public interface IMemoryAllocator<T>
    {
        void Resize(ref T[] items, int requiredLength);
    }
}