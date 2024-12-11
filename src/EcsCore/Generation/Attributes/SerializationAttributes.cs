using System;

namespace EcsCore.Serialization
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class MinMaxValueAttribute : Attribute
    {
        public readonly long MaxValue;
        public readonly long MinValue;

        public MinMaxValueAttribute(long minValue, long maxValue)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FloatMinMaxValueAttribute : Attribute
    {
        public readonly float MaxValue;
        public readonly float MinValue;
        public readonly float Precision;
        
        public FloatMinMaxValueAttribute(float minValue, float maxValue, float precision)
        {
            MaxValue = maxValue;
            MinValue = minValue;
            Precision = precision;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class IgnoreSerializationAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct, AllowMultiple = false)]
    public class DiffableAttribute : Attribute
    {
    }
}