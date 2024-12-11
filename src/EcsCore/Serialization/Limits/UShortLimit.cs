namespace EcsCore.Serialization.Limits
{
    public struct UShortLimit
    {
        public readonly ushort Min;
        public readonly ushort Max;
        public readonly int BitCount;

        public UShortLimit(ushort min, ushort max)
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