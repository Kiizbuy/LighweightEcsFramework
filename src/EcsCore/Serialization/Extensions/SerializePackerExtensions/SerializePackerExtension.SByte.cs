using System.Runtime.CompilerServices;
using EcsCore.Serialization.Limits;

namespace EcsCore.Serialization.Extensions
{
    public static partial class SerializePackerExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReadSByte(this ISerializePacker reader, sbyte min, sbyte max)
        {
            return ReadSByte(reader, new SByteLimit(min, max));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReadSByte(this ISerializePacker reader, in SByteLimit limit)
        {
            var value = reader.ReadSByte(limit.BitCount);
            return (sbyte)(value + limit.Min);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReadSByte(this ISerializePacker reader, sbyte baseline)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                return reader.ReadSByte();
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReadSByte(this ISerializePacker reader, sbyte baseline, in SByteLimit limit)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                return reader.ReadSByte(limit);
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte ReadByte(this ISerializePacker reader, 
            sbyte baseline, 
            in SByteLimit limit,
            in SByteLimit diffLimit)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                var isDiff = reader.ReadBool();

                if (isDiff)
                {
                    var diff = reader.ReadSByte(diffLimit);
                    return (sbyte)(baseline + diff);
                }

                return reader.ReadSByte(limit);
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this ISerializePacker writer, sbyte value, in SByteLimit limit)
        {
#if DEBUG
            if (value < limit.Min || value > limit.Max)
            {
            }
#endif

            writer.Write((uint)(value - limit.Min), limit.BitCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValueIfChanged(this ISerializePacker packer, sbyte baseline, sbyte updated)
        {
            if (baseline == updated)
            {
                packer.Write(false);
            }
            else
            {
                packer.Write(true);
                packer.Write(updated);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValueIfChanged(this ISerializePacker packer, 
            sbyte baseline, 
            sbyte updated,
            in SByteLimit limit)
        {
            if (baseline.Equals(updated))
            {
                packer.Write(false);
            }
            else
            {
                packer.Write(true);
                packer.Write(updated, limit);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteDiffIfChanged(this ISerializePacker writer,
            sbyte baseline,
            sbyte updated,
            in SByteLimit limit,
            in SByteLimit diffLimit)
        {
            if (baseline.Equals(updated))
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);

                var diff = (sbyte)(updated - baseline);

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