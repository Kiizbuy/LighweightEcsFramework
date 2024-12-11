using System.Runtime.CompilerServices;
using EcsCore.Serialization.Limits;

namespace EcsCore.Serialization.Extensions
{
    public static partial class SerializePackerExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUShort(this ISerializePacker packer, ushort min, ushort max) 
            => packer.ReadUShort(new UShortLimit(min, max));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUShort(this ISerializePacker packer, UShortLimit limit)
        {
            var value = packer.ReadUShort(limit.BitCount);
            return (ushort)(value + limit.Min);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUShort(this ISerializePacker packer, ushort baseline)
        {
            var isChanged = packer.ReadBool();
            if (isChanged)
            {
                return packer.ReadUShort();
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUShort(this ISerializePacker packer, ushort baseline, UShortLimit limit)
        {
            var isChanged = packer.ReadBool();
            if (isChanged)
            {
                return packer.ReadUShort(limit);
            }

            return baseline;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUShort(this ISerializePacker packer, ushort baseline, UShortLimit limit, UShortLimit diffLimit)
        {
            var isChanged = packer.ReadBool();
            if (isChanged)
            {
                var isDiff = packer.ReadBool();

                if (isDiff)
                {
                    var diff = packer.ReadUShort(diffLimit);
                    return (ushort)(baseline + diff);
                }

                return packer.ReadUShort(limit);
            }

            return baseline;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this ISerializePacker packer, ushort value, UShortLimit limit)
        {
#if DEBUG
            if (value < limit.Min || value > limit.Max)
            {
            }
#endif
        
            packer.Write((uint)(value - limit.Min), limit.BitCount);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValueIfChanged(this ISerializePacker packer, ushort baseline, ushort updated)
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
        public static void WriteValueIfChanged(this ISerializePacker packer, ushort baseline, ushort updated, UShortLimit limit)
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
        public static void WriteDiffIfChanged(this ISerializePacker packer, ushort baseline, ushort updated, UShortLimit limit, UShortLimit diffLimit)
        {
            if (baseline.Equals(updated))
            {
                packer.Write(false);
            }
            else
            {
                packer.Write(true);
            
                var diff = (ushort)(updated - baseline);

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