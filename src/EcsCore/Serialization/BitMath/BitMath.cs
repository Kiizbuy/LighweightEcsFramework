﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EcsCore.Serialization.BitMath
{
    public static class BitMath
    {
        private static ReadOnlySpan<byte> Log2DeBruijn => new byte[32]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static byte FloatToByte(float val)
        {
            return (byte) Math.Min(byte.MaxValue, (val * 0.5f + 0.5f) * byte.MaxValue);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		
        public static short FloatToShort(float val)
        {
            var res = (short)Math.Max(short.MinValue, Math.Min(short.MaxValue, val * short.MaxValue));
            return res;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		
        public static float FloatFromByte(byte val)
        {
            return ((float) val / byte.MaxValue - 0.5f) * 2.0f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		
        public static float FloatFromShort(short val)
        {
            var res = (float) val / short.MaxValue;
            return res;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static int FloatToInt(float val)
        {
            var res = (int)Math.Max(int.MinValue, Math.Min(int.MaxValue, val * int.MaxValue));
            return res;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static float FloatFromInt(int val)
        {
            var res = (float) val / int.MaxValue;
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Ceiling(int a, int b)
        {
            var (quotient, remainder) = DivRem(a, b);
            if (remainder == 0)
            {
                return quotient;
            }

            return quotient + 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (int Quotient, int Remainder) DivRem(int left, int right)
        {
            int quotient = left / right;
            return (quotient, left - quotient * right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int BitsRequired(uint range)
        {
#if NETSTANDARD2_0 || NETSTANDARD2_1
            return range == 0 ? 1 : Log2(range) + 1;
#else
            return range == 0 ? 1 : BitOperations.Log2(range) + 1;
#endif
        }

        public static int Log2(uint value)
        {
            // The 0->0 contract is fulfilled by setting the LSB to 1.
            // Log(1) is 0, and setting the LSB for values > 1 does not change the log2 result.
            value |= 1;

            // No AggressiveInlining due to large method size
            // Has conventional contract 0->0 (Log(0) is undefined)

            // Fill trailing zeros with ones, eg 00010010 becomes 00011111
            value |= value >> 01;
            value |= value >> 02;
            value |= value >> 04;
            value |= value >> 08;
            value |= value >> 16;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
                ref MemoryMarshal.GetReference(Log2DeBruijn),
                // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                (IntPtr)(int)((value * 0x07C4ACDDu) >> 27));
        }
    }
}