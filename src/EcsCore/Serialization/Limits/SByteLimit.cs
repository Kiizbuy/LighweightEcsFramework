namespace EcsCore.Serialization.Limits
{
    public readonly struct SByteLimit
    {
        public readonly sbyte Min;
        public readonly sbyte Max;
        public readonly int BitCount;

        public SByteLimit(sbyte min, sbyte max)
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