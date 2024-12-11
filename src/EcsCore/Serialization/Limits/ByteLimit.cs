namespace EcsCore.Serialization.Limits
{
    public readonly struct ByteLimit
    {
        public readonly byte Min;
        public readonly byte Max;
        public readonly int BitCount;

        public ByteLimit(byte min, byte max)
        {
            if (min > max)
            {
            }
        
            var range = max - min;
            BitCount = BitMath.BitMath.BitsRequired((uint)range);
            Min = min;
            Max = max;
        }
    }
}