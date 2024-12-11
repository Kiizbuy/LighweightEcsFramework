using System;
using System.Runtime.CompilerServices;
using EcsCore.Serialization.Limits;

namespace EcsCore.Serialization.Extensions
{
    public partial class SerializePackerExtension
    {
        public const float DefaultFloatPrecision = 0.0000001f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloat(this ISerializePacker reader, float min, float max, float precision)
        {
            var result = reader.ReadFloat(new FloatLimit(min, max, precision));
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloat(this ISerializePacker reader, in FloatLimit limit)
        {
            var integerValue = reader.ReadUInt(limit.BitCount);
            var normalizedValue = integerValue / (float)limit.MaxIntegerValue;

            return normalizedValue * limit.Delta + limit.Min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloat(this ISerializePacker reader, float baseline)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                return reader.ReadFloat();
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloat(this ISerializePacker reader, float baseline, in FloatLimit limit)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                return reader.ReadFloat(limit);
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloat(this ISerializePacker reader, float baseline, in FloatLimit limit,
            FloatLimit diffLimit)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                var isDiff = reader.ReadBool();

                if (isDiff)
                {
                    var diff = reader.ReadFloat(diffLimit);
                    return baseline + diff;
                }

                return reader.ReadFloat(limit);
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this ISerializePacker writer, float value, float min, float max,
            float precision)
        {
            writer.Write(value, new FloatLimit(min, max, precision));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this ISerializePacker writer, float value, in FloatLimit limit)
        {
#if DEBUG
            if (value < limit.Min || value > limit.Max)
            {
            }
#endif

            var normalizedValue = Clamp((value - limit.Min) / limit.Delta, 0, 1);
            var integerValue = (uint)Math.Floor(normalizedValue * limit.MaxIntegerValue + 0.5f);

            writer.Write(integerValue, limit.BitCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValueIfChanged(this ISerializePacker writer, float baseline, float updated)
        {
            var diff = updated - baseline;
            if (Math.Abs(diff) < DefaultFloatPrecision)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(updated);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValueIfChanged(this ISerializePacker writer, 
            float baseline, 
            float updated,
            in FloatLimit limit)
        {
            if (Math.Abs(updated - baseline) < limit.Precision)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
                writer.Write(updated, limit);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDiffIfChanged(this ISerializePacker writer, 
            float baseline, 
            float updated,
            in FloatLimit limit, 
            in FloatLimit diffLimit)
        {
            var diff = updated - baseline;
            if (Math.Abs(diff) < limit.Precision)
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);

                if (diffLimit.Min < diff && diff < diffLimit.Max)
                {
                    // if diff inside of diff limit, then we will write diff
                    writer.Write(true);
                    writer.Write(diff, diffLimit);
                }
                else
                {
                    // otherwise we will write updated value
                    writer.Write(false);
                    writer.Write(updated, limit);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float Clamp(float value, float min, float max)
        {
            if (min > max)
            {
            }

            if (value < min)
            {
                return min;
            }
            else if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}