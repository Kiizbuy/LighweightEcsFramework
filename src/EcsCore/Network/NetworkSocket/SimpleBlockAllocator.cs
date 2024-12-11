using System.Runtime.CompilerServices;

namespace EcsCore.Serialization
{
    public sealed class SimpleBlockAllocator : IMemoryAllocator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Allocate(int size)
        {
            return new byte[size];
        }
    }
}