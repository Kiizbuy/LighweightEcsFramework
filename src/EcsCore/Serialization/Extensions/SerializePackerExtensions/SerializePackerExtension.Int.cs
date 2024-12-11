using System.Runtime.CompilerServices;
using EcsCore.Serialization.Limits;

namespace EcsCore.Serialization.Extensions
{
    public static partial class SerializePackerExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt(this ISerializePacker packer, int min, int max) =>
            packer.ReadInt(new IntLimit(min, max));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt(this ISerializePacker packer, in IntLimit limit)
        {
            var value = packer.ReadInt(limit.BitCount);
            return value + limit.Min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt(this ISerializePacker packer, int baseline)
        {
            var isChanged = packer.ReadBool();
            if (isChanged)
            {
                return packer.ReadInt();
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt(this ISerializePacker packer, int baseline, in IntLimit limit)
        {
            var isChanged = packer.ReadBool();
            if (isChanged)
            {
                return packer.ReadInt(limit);
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt(this ISerializePacker packer, int baseline, in IntLimit limit, in IntLimit diffLimit)
        {
            var isChanged = packer.ReadBool();
            if (isChanged)
            {
                var isDiff = packer.ReadBool();

                if (isDiff)
                {
                    var diff = packer.ReadInt(diffLimit);
                    return baseline + diff;
                }

                return packer.ReadInt(limit);
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this ISerializePacker writer, int value, int min, int max)
        {
            writer.Write(value, new IntLimit(min, max));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this ISerializePacker writer, int value, in IntLimit limit)
        {
#if DEBUG
            if (value < limit.Min || value > limit.Max)
            {
            }
#endif

            writer.Write((uint)(value - limit.Min), limit.BitCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValueIfChanged(this ISerializePacker packer, int baseline, int updated)
        {
            if (baseline.Equals(updated))
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
            int baseline, 
            int updated,
            in IntLimit limit)
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
        public static void WriteDiffIfChanged(this ISerializePacker packer,
            int baseline,
            int updated,
            in IntLimit limit,
            in IntLimit diffLimit)
        {
            if (baseline.Equals(updated))
            {
                packer.Write(false);
            }
            else
            {
                packer.Write(true);

                var diff = updated - baseline;

                if (diffLimit.Min < diff && diff < diffLimit.Max)
                {
                    // if diff inside of diff limit, then we will write diff
                    packer.Write(true);
                    packer.Write(diff, diffLimit);
                }
                else
                {
                    // otherwise we will write updated value
                    packer.Write(false);
                    packer.Write(updated, limit);
                }
            }
        }
    }
}