using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace CodeGenerator
{
    internal static class SizeAndBitsCountUtils
    {
        private static readonly Dictionary<string, KeyValuePair<string, int>> BitsToSize;
        private static readonly HashSet<string> PrimitiveTypeNames;
        
        static SizeAndBitsCountUtils()
        {
            PrimitiveTypeNames = new HashSet<string>
            {
                "int",
                "uint",
                "byte",
                "sbyte",
                "bool",
                "float",
                "short",
                "ushort",
                "ulong",
                "long",
                "string"
            };
            
            BitsToSize = new Dictionary<string, KeyValuePair<string, int>>()
            {
                {"int", new KeyValuePair<string, int>("Int", (sizeof(int) * 8))}, 
                {"uint", new KeyValuePair<string, int>("UInt", (sizeof(int) * 8))}, 
                {"byte", new KeyValuePair<string, int>("Byte", (sizeof(byte) * 8))}, 
                {"sbyte", new KeyValuePair<string, int>("Byte", (sizeof(sbyte) * 8))}, 
                {"bool", new KeyValuePair<string, int>("Bool", -1)}, 
                {"float", new KeyValuePair<string, int>("Float", -1)}, 
                {"short", new KeyValuePair<string, int>("Short", (sizeof(short) * 8))},
                {"ushort", new KeyValuePair<string, int>("UShort", (sizeof(ushort) * 8))},
                {"ulong", new KeyValuePair<string, int>("ULong", (sizeof(ulong) * 8))},
                {"long", new KeyValuePair<string, int>("Long", (sizeof(long) * 8))},
                {"string", new KeyValuePair<string, int>("String", -1)},
            };
        }
        
        internal static KeyValuePair<string, int> GetSizeAndBitsCountText(string typeToUseStringLetiral)
        {
            if (BitsToSize.ContainsKey(typeToUseStringLetiral.RemoveEmptySpaces()))
            {
                return BitsToSize[typeToUseStringLetiral];
            }
            
            throw new TypeAccessException($"Unsupported type in state {typeToUseStringLetiral}");
        }
        
        internal static bool IsPackerSerializableType(string type)
        {
            return PrimitiveTypeNames.Contains(type.RemoveEmptySpaces());
        }
    }
}