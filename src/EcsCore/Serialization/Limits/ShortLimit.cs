namespace EcsCore.Serialization.Limits
{
    public readonly struct ShortLimit
    {
        public readonly short Min;
        public readonly short Max;
        public readonly int BitCount;

        public ShortLimit(short min, short max)
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