using System;
using System.IO;

namespace NetCodeUtils
{
    public class BitsPacker : IPacker
    {
        private byte[] _dataBuffer;
        private const int InitialSize = 64;
        private MemoryStream _stream;

        public BitsPacker() : this(InitialSize)
        {
        }

        public BitsPacker(MemoryStream stream) : this(InitialSize)
        {
            _stream = stream;
        }

        public long GetStreamPosition()
        {
            return _stream.Position;
        }

        public void SetStream(MemoryStream stream) => _stream = stream;
        
        public void Clear()
        {
            _stream = null;
            _dataBuffer = null;
        }

        public BitsPacker(int initialSize = InitialSize)
        {
            _dataBuffer = new byte[initialSize];
        }
        
        public byte[] Flush()
        {
            _stream.Flush();
            return _stream.GetBuffer();
        }

        public void WriteInt(int value)
        {
            _stream.Write(BitConverter.GetBytes(value), 0, sizeof(int));
        }

        public void WriteUint(uint value)
        {
            _stream.Write(BitConverter.GetBytes(value), 0, sizeof(uint));
        }

        public void WriteShort(short value)
        {
            _stream.Write(BitConverter.GetBytes(value), 0, sizeof(short));
        }

        public void WriteBool(bool value)
        {
            _stream.WriteByte((byte) (value ? 1 : 0));
        }

        public void WriteFloat(float value)
        {
            _stream.Write(BitConverter.GetBytes(value), 0, sizeof(float));
        }

        public int ReadInt()
        {
            _stream.Read(_dataBuffer, 0, sizeof(int));
            return BitConverter.ToInt32(_dataBuffer, 0);
        }

        public uint ReadUint()
        {
            _stream.Read(_dataBuffer, 0, sizeof(uint));
            return BitConverter.ToUInt32(_dataBuffer, 0);
        }

        public float ReadFloat()
        {
            _stream.Read(_dataBuffer, 0, sizeof(float));
            return BitConverter.ToSingle(_dataBuffer, 0);
        }

        public bool ReadBool()
        {
            return _stream.ReadByte() > 0;;
        }
    }
}