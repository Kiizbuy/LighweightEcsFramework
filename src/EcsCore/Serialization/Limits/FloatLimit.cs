using System;
using EcsCore.Serialization.Extensions;

namespace EcsCore.Serialization.Limits
{
    public readonly struct FloatLimit
    {
        public readonly float Min;
        public readonly float Max;
        public readonly float Precision;
        public readonly float Delta;
        public readonly uint MaxIntegerValue;
        public readonly int BitCount;
    
        public FloatLimit(float min, float max, float precision = SerializePackerExtension.DefaultFloatPrecision)
        {
            if (min >= max)
            {
                throw new Exception();
            }
            
            Min = min;
            Max = max;
            Precision = precision;
            Delta = max - min;
            var values = Delta / precision;
            MaxIntegerValue = (uint)Math.Ceiling(values);
            BitCount = BitMath.BitMath.BitsRequired(MaxIntegerValue);
        }
    }
}