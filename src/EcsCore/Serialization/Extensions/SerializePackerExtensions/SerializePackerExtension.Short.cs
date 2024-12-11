using System.Runtime.CompilerServices;
using EcsCore.Serialization.Limits;

namespace EcsCore.Serialization.Extensions
{
    public static partial class SerializePackerExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this ISerializePacker writer, short value, short min, short max)
        {
            Write(writer, value, new ShortLimit(min, max));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this ISerializePacker writer, short value, in ShortLimit limit)
        {
#if DEBUG
            if (value < limit.Min || value > limit.Max)
            {
            }
#endif

            writer.Write((uint)(value - limit.Min), limit.BitCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValueIfChanged(this ISerializePacker writer, short baseline, short updated)
        {
            if (baseline == updated)
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
            short baseline, 
            short updated,
            in ShortLimit limit)
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
        public static void WriteDiffIfChanged(this ISerializePacker writer,
            short baseline,
            short updated,
            in ShortLimit limit,
            in ShortLimit diffLimit)
        {
            if (baseline.Equals(updated))
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);

                var diff = (short)(updated - baseline);

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
        public static short ReadShort(this ISerializePacker reader, ShortLimit limit)
        {
            var value = reader.ReadShort(limit.BitCount);
            return (short)(value + limit.Min);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadShort(this ISerializePacker reader, short baseline)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                return reader.ReadShort();
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadShort(this ISerializePacker reader, short baseline, ShortLimit limit)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                return reader.ReadShort(limit);
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadShort(this ISerializePacker reader, short baseline, ShortLimit limit,
            ShortLimit diffLimit)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                var isDiff = reader.ReadBool();

                if (isDiff)
                {
                    var diff = reader.ReadShort(diffLimit);
                    return (short)(baseline + diff);
                }

                return reader.ReadShort(limit);
            }

            return baseline;
        }
    }
}