using System.Runtime.CompilerServices;

namespace EcsCore.Serialization
{
    public sealed class SimpleMemoryPool : IMemoryPool
    {
        private readonly IMemoryAllocator _allocator;
        
        public SimpleMemoryPool(IMemoryAllocator allocator)
        {
            _allocator = allocator;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Rent(int minimumSize)
        {
            return _allocator.Allocate(minimumSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(byte[] chunk)
        {
            // This simple pool does not care of releasing your shit. ;)
        }
    }
}