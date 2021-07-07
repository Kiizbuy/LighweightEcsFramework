using System;

namespace EcsCore.MemoryAllocation
{
    public sealed class BlockMemoryAllocator<T> : IMemoryAllocator<T>
    {
        private const int DefaultBlockSize = 2;

        private readonly int _blockSize;

        public BlockMemoryAllocator() : this(DefaultBlockSize)
        {
        }

        public BlockMemoryAllocator(int blockSize)
        {
            if (blockSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(blockSize));
            }
            _blockSize = blockSize;
        }

        public void Resize(ref T[] array, int requiredMinSize)
        {
            var blockCount = requiredMinSize / _blockSize;
            if (requiredMinSize % _blockSize > 0)
            {
                ++blockCount;
            }
            Array.Resize(ref array, blockCount * _blockSize);
        }
    }
}