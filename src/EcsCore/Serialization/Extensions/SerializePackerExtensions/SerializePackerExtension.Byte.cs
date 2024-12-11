using System.Runtime.CompilerServices;
using EcsCore.Serialization.Limits;

namespace EcsCore.Serialization.Extensions
{
    public static partial class SerializePackerExtension
    {
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(this ISerializePacker reader, byte min, byte max)
        {
            return ReadByte(reader, new ByteLimit(min, max));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(this ISerializePacker reader, in ByteLimit limit)
        {
            var value = reader.ReadByte(limit.BitCount);
            return (byte)(value + limit.Min);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(this ISerializePacker reader, byte baseline)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                return reader.ReadByte();
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(this ISerializePacker reader, byte baseline, in ByteLimit limit)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                return reader.ReadByte(limit);
            }

            return baseline;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByte(this ISerializePacker reader, byte baseline, in ByteLimit limit, in ByteLimit diffLimit)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                var isDiff = reader.ReadBool();

                if (isDiff)
                {
                    var diff = reader.ReadByte(diffLimit);
                    return (byte)(baseline + diff);
                }

                return reader.ReadByte(limit);
            }

            return baseline;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this ISerializePacker writer, byte value, in ByteLimit limit)
        {
#if DEBUG
            if (value < limit.Min || value > limit.Max)
            {
            }
#endif
        
            writer.Write((uint)(value - limit.Min), limit.BitCount);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValueIfChanged(this ISerializePacker packer, byte baseline, byte updated)
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
        public static void WriteValueIfChanged(this ISerializePacker packer, byte baseline, byte updated, ByteLimit limit)
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
        public static void WriteDiffIfChanged(this ISerializePacker writer, byte baseline, byte updated, ByteLimit limit, ByteLimit diffLimit)
        {
            if (baseline.Equals(updated))
            {
                writer.Write(false);
            }
            else
            {
                writer.Write(true);
            
                var diff = (byte)(updated - baseline);

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