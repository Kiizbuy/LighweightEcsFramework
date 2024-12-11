using System.Runtime.CompilerServices;

namespace EcsCore.Serialization.Extensions
{
    public static partial class SerializePackerExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FFloat32 ReadFFloat32(this ISerializePacker packer)
        {
            var fixedFloat = new FFloat32(0, 0);
            fixedFloat.RawValue = packer.ReadLong();
            fixedFloat.Scale = packer.ReadInt();
            return fixedFloat;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FFloat32 ReadFFloat32(this ISerializePacker reader, FFloat32 baseline)
        {
            var isChanged = reader.ReadBool();
            if (isChanged)
            {
                return reader.ReadByte();
            }

            return baseline;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this ISerializePacker packer, FFloat32 value)
        {
            packer.Write(value.RawValue);
            packer.Write(value.Scale);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WriteValueIfChanged(this ISerializePacker writer, FFloat32 baseline, FFloat32 updated)
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
    }
}