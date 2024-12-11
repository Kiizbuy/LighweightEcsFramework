namespace EcsCore.Serialization.Limits
{
    public readonly struct IntLimit
    {
        public readonly int Min;
        public readonly int Max;
        public readonly int BitCount;

        public IntLimit(int min, int max)
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