using System.IO;

namespace EcsCore.Serialization
{
    public interface ISerializePacker
    {
        int GetSize();
        long GetStreamPosition();
        byte[] GetBuffer();
        void Clear();
        void SkipRawBits(int numbits);
        void SkipRawBytes(int count);
        void Write(byte value, int bits = sizeof(byte) * 8);
        void Write(sbyte value, int bits = sizeof(byte) * 8);
        void Write(byte[] value);
        void Write(bool value);
        void Write(short value, int bits = sizeof(short) * 8);
        void Write(ushort value, int bits = sizeof(short) * 8);
        void Write(int value, int bits = sizeof(int) * 8);
        void Write(uint value, int bits = sizeof(uint) * 8);
        void Write(long value, int bits = sizeof(long) * 8);
        void Write(ulong value, int bits = sizeof(ulong) * 8);
        void Write(float value);
        void Write(string value);
        int ReadInt(int bits = sizeof(int) * 8);
        uint ReadUInt(int bits = sizeof(int) * 8);
        long ReadLong(int bits = sizeof(long) * 8);
        ulong ReadULong(int bits = sizeof(ulong) * 8);
        float ReadFloat();
        byte ReadByte(int bits = sizeof(byte) * 8);
        sbyte ReadSByte(int bits = sizeof(byte) * 8);
        short ReadShort(int bits = sizeof(short) * 8);
        ushort ReadUShort(int bits = sizeof(short) * 8);
        bool ReadBool();
        string ReadString();
    }
}