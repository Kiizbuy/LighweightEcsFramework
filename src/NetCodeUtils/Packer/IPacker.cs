using System.IO;

namespace NetCodeUtils
{
    public interface IPacker
    {
        long GetStreamPosition();
        byte[] Flush();
        void SetStream(MemoryStream memoryStream);
        void Clear();
        void WriteFloat(float value);
        void WriteInt(int value);
        void WriteUint(uint value);
        void WriteShort(short value);
        void WriteBool(bool value);
        int ReadInt();
        uint ReadUint();
        float ReadFloat();
        bool ReadBool();
    }
}