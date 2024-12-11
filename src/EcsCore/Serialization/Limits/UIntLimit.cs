namespace EcsCore.Serialization.Limits
{
    public readonly struct UIntLimit
    {
        public readonly uint Min;
        public readonly uint Max;
        public readonly int BitCount;

        public UIntLimit(uint min, uint max)
        {
            if (min > max)
            {
            }
        
            var range = max - min;
            BitCount = BitMath.BitMath.BitsRequired(range);
            Min = min;
            Max = max;
        }
    }
}