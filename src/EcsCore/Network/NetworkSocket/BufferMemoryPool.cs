using System.Buffers;
using System.Runtime.CompilerServices;

namespace EcsCore.Serialization
{
    public class BufferMemoryPool : IMemoryPool
    {
        private readonly ArrayPool<byte> _arrayPool;

        public BufferMemoryPool() : this(ArrayPool<byte>.Create())
        {
        }
        
        public BufferMemoryPool(ArrayPool<byte> arrayPool)
        {
            _arrayPool = arrayPool;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte[] Rent(int minimumSize)
        {
            lock (_arrayPool)
            {
                return _arrayPool.Rent(minimumSize);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release(byte[] chunk)
        {
            if (chunk == null)
                return;
            
            lock (_arrayPool)
            {
                _arrayPool.Return(chunk);
            }
        }
    }
}