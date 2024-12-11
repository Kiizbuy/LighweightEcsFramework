using System;
using System.Collections.Generic;

namespace CodeGenerator
{
    public class BitsOperationsUtils
    {
        internal static long GetMinMaxSize(KeyValuePair<long, long>minmax)
        {
            var max = Math.Max(Math.Abs(minmax.Key), Math.Abs(minmax.Value));
            return GetBitsCount(max);
        }
        
        internal static int GetBitsCount(long num)
        {
            var bits = 0;
            while (num > 0)
            {
                num /= 2;
                bits++;
            }

            return bits;
        }
    }
}