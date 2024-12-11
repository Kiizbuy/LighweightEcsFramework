using System;
using System.Collections.Generic;
using System.Threading;
using EcsCore.Components;

namespace EcsCore
{
    internal readonly struct EcsTypeMask
    {
        public readonly int TypeIndex;
        public readonly int TypeBitMask;

        public EcsTypeMask(int typeIndex, int typeBitMask)
        {
            TypeIndex = typeIndex;
            TypeBitMask = typeBitMask;
        }
    }
    
    internal static class EcsComponentTypes<T> where T: struct, IComponentData
    {
        public static int TypeIndex;
        public static int TypeHash;
        public static int TypeBitMask;

        static EcsComponentTypes()
        {
            WarmUp();
        }

        private static void WarmUp()
        {
            TypeIndex = Interlocked.Increment(ref ComponentMetaDataInfos.ComponentTypesCount);
            TypeHash = typeof(T).Name.GenerateHash();
            TypeBitMask = 1 << (TypeIndex + 1);
            ComponentMetaDataInfos.ComponentHash.Add(TypeHash);
        }
    }
    
    #region Remove That Shit in future
    internal static class EcsComponentViewTypes<T> where T: struct, IViewComponentData
    {
        public static int TypeIndex;
        public static int TypeBitMask;

        static EcsComponentViewTypes()
        {
            WarmUp();
        }

        private static void WarmUp()
        {
            TypeIndex = Interlocked.Increment(ref ComponentMetaDataInfos.ComponentViewTypesCount);
            TypeBitMask = 1 << (TypeIndex + 1);
        }
    }
    #endregion

    internal sealed class ComponentMetaDataInfos
    {
        internal static int ComponentTypesCount = -1;
        internal static int ComponentViewTypesCount = -1;
        internal static readonly HashSet<int> ComponentHash = new HashSet<int>();

    }
    
    public static class HashGenerator
    {
        public static int GenerateHash(this string input)
        {
            return GenerateHashForString(input);
        }

        public static int GenerateHashForType(this Type type)
        {
            return GenerateHashForString(type.Name);
        }

        public static int GenerateHashForString(string input)
        {
            int index = input.Length + input[0].GetHashCode();
            int half = input.Length / 2;

            for (int i = 0; i < input.Length; i++)
            {
                char charC = input[i];
                index += charC.GetHashCode();
                index += input.Length;

                if (i > half)
                    index += -1531;

                if (charC == 'a')
                    index += -51;
                else if (charC == 'o')
                    index += 33;
                else if (charC == 'e')
                    index += -7;
                else if (charC == '0')
                    index += 2 * i;
                else if (charC == '1')
                    index += 3 * i;
                else if (charC == '2')
                    index += 5 * i;
            }

            return index;
        }
    }
}