using System;

namespace CodeGenerator.TestDatas
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public class FixedArrayGenerationAttribute : Attribute
    {
        public readonly int MaxSize;

        public FixedArrayGenerationAttribute(int maxSize)
        {
            MaxSize = maxSize;
        }
    }
}