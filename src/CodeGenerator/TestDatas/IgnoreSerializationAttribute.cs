using System;

namespace CodeGenerator.TestDatas
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class IgnoreSerializationAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class MinMaxValueAttribute : Attribute
    {
        public readonly long MinValue;
        public readonly long MaxValue;

        public MinMaxValueAttribute(long minValue, long maxValue)
        {
            MaxValue = maxValue;
            MinValue = minValue;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FloatMinMaxValueAttribute : Attribute
    {
        public readonly float MinValue;
        public readonly float MaxValue;
        public readonly float Precision;
        
        public FloatMinMaxValueAttribute(float minValue, float maxValue, float precision)
        {
            MaxValue = maxValue;
            MinValue = minValue;
            Precision = precision;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class MaskableAttribute : Attribute
    {
    }
}