using System.Runtime.CompilerServices;
using EcsCore.Serialization.Limits;

namespace EcsCore.Serialization.Extensions
{
    public static partial class SerializePackerExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt(this ISerializePacker reader, uint min, uint max) =>
            reader.ReadUInt(new UIntLimit(min, max));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt(this ISerializePacker reader, UIntLimit limit)
        {
            var value = reader.ReadUInt(limit.BitCount);
            return value + limit.Min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt(this ISerializePacker reader, uint baseline)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                return reader.ReadUInt();
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt(this ISerializePacker reader, uint baseline, UIntLimit limit)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                return reader.ReadUInt(limit);
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt(this ISerializePacker reader, uint baseline, UIntLimit limit, UIntLimit diffLimit)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                var isDiff = reader.ReadBool();

                if (isDiff)
                {
                    var diff = reader.ReadUInt(diffLimit);
                    return baseline + diff;
                }

                return reader.ReadUInt(limit);
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this ISerializePacker writer, uint value, uint min, uint max)
        {
            writer.Write(value, new UIntLimit(min, max));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this ISerializePacker writer, uint value, UIntLimit limit)
        {
#if DEBUG
            if (value < limit.Min || value > limit.Max)
            {
                // ThrowHelper.ThrowArgumentOutOfRangeException();
            }
#endif
            writer.Write(value - limit.Min, limit.BitCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValueIfChanged(this ISerializePacker writer, uint baseline, uint updated)
        {
            if (baseline.Equals(updated))
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
        public static void WriteValueIfChanged(this ISerializePacker writer, uint baseline, uint updated, UIntLimit limit)
        {
            if (baseline.Equals(updated))
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
        public static void WriteDiffIfChanged(this ISerializePacker writer, uint baseline, uint updated, UIntLimit limit,
            UIntLimit diffLimit)
        {
            if (baseline.Equals(updated))
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);

                var diff = updated - baseline;

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
    }
}