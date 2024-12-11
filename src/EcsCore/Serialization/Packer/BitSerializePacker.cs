using System;
using System.Runtime.CompilerServices;
using System.Text;
using EcsCore.Network;
using EcsCore.Network.Buffers;
using EcsCore.Network.NetworkSocket;

namespace EcsCore.Serialization
{
    public class BitSerializePacker : ISerializePacker
    {
        private const int GrowingFactor = 2;
        
        private byte[] _buffer;
        private bool _isBufferInternallyManaged;

        private int _bitsPointer;
        private int _bytePointer;
        private byte _currentByte;

        private IMemoryPool _pool;

        public void SetBuffer(IMemoryPool pool, int size)
        {
            _pool = pool;
            _isBufferInternallyManaged = true;
            _buffer = pool.Rent(size);
        }

        public void SetBuffer(ref byte[] buffer)
        {
            if(buffer.Length <= 0) throw new Exception("Buffer length is not valid");
            _buffer = buffer;
            _bytePointer = 0;
            _currentByte = _buffer[_bytePointer];
            _isBufferInternallyManaged = false;
        }
        
        public void SkipRawBits(int numbits)
        {
            // TODO: implement this properly
            while (numbits >= 32)
            {
                ReadBits(32);
                numbits -= 32;
            }
            ReadBits(numbits);
        }

        public void SkipRawBytes(int count)
        {
            SkipRawBits(count * 8);
        }

        public void SetBuffer(IMemoryPool pool, ref byte[] buffer)
        {
            _pool = pool;
            var newBuffer = pool.Rent(buffer.Length);
            Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
            _buffer = newBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte ReadBits(int bitsAmount)
        {
            if (_bitsPointer == 0 && bitsAmount == 8)
            {
                var tmp = _currentByte;
                if (_bytePointer < _buffer.Length - 1)
                {
                    _bytePointer += 1;
                    _currentByte = _buffer[_bytePointer];
                }
                return tmp;
            }

            var freeBitsMaskOffset = (8 - _bitsPointer - bitsAmount);
            freeBitsMaskOffset = (freeBitsMaskOffset >= 0) ? freeBitsMaskOffset : 0;
            // Remaining bits to read on the next byte in the buffer.
            var leftBitsToRead = bitsAmount - (8 - _bitsPointer);

            var data = (byte)((_currentByte & ((0xFF << _bitsPointer) & (0xFF >> freeBitsMaskOffset))) >> _bitsPointer);

            if (leftBitsToRead > 0)
            {
                _bytePointer += 1;
                _currentByte = _buffer[_bytePointer];

                var alreadyReadBits = (bitsAmount - leftBitsToRead);
                data = (byte)(data | ((_currentByte << alreadyReadBits) & ((0xFF << alreadyReadBits) & (0xFF >> (8 - bitsAmount)))));
                _bitsPointer = leftBitsToRead;
            }
            else
            {
                _bitsPointer += bitsAmount;
            }

            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteBits(byte data, int bitsAmount)
        {
            if (bitsAmount <= 0) return;
            if (bitsAmount > 8) bitsAmount = 8;

            if (_bitsPointer == 0 && bitsAmount == 8)
            {
                _currentByte = data;
                _buffer[_bytePointer] = data;
                _bytePointer += 1;
                return;
            }
            
            // Left bits in the current byte in the buffer.
            //int leftBits = 8 - _bitsPointer - bitsAmount;
            // If it is < 0, we have to move to the next byte.
            //leftBits = (leftBits < 0) ? 0 : leftBits;
            // Remaining bits to write to the next byte.
            var leftBitsToWrite = bitsAmount - (8 - _bitsPointer);

            // This is the mask to preserve old written bits (0xFF << _bitsPointer)
            // and to allow writing of new ones.
            var mask = (0xFF << _bitsPointer); // & (0xFF >> leftBits); // Do we really need to mask out bits > _bitsPointer + leftBits?

            // Write to the current byte.
            _currentByte = (byte)((_currentByte & (0xFF >> (8 - _bitsPointer))) | ((data << _bitsPointer) & mask));
            // Write the current byte to the buffer.
            _buffer[_bytePointer] = _currentByte;

            // If we have left bits to write, we have to advance the buffer pointer.
            if (leftBitsToWrite > 0)
            {
                _bytePointer += 1;
                _currentByte = 0x00;

                _currentByte = (byte)((_currentByte) | ((data >> (bitsAmount - leftBitsToWrite)) & (0xFF >> 8 - leftBitsToWrite)));
                _buffer[_bytePointer] = _currentByte;

                _bitsPointer = leftBitsToWrite;
            }
            // Else we can just increment our bits pointer.
            else
            {
                _bitsPointer += bitsAmount;
                if (_bitsPointer >= 8)
                {
                    _bitsPointer = 0;
                    _bytePointer += 1;
                    _currentByte = 0x00;
                }
            }
        }

        public byte ReadByte(int bits = sizeof(byte) * 8)
        {
            return ReadBits(bits);
        }

        public sbyte ReadSByte(int bits = sizeof(byte) * 8)
        {
            return (sbyte)ReadBits(bits);
        }

        public byte[] ReadBytes(int length)
        {
            var bytes = new byte[length];

            if (_bitsPointer == 0)
            {
                Buffer.BlockCopy(_buffer, _bytePointer, bytes, 0, length);
                return bytes;
            }

            for (var index = 0; index < length; index++)
            {
                bytes[index] = ReadBits(8);
            }
            return bytes;
        }

        public bool ReadBool()
        {
            return ReadBits(1) == 1;
        }

        public short ReadShort(int bits = sizeof(short) * 8)
        {
            return (short)ReadUShort(bits);
        }

        public ushort ReadUShort(int bits = sizeof(short) * 8)
        {
            if (bits <= 8)
            {
                return (ushort)ReadBits(bits);
            }
            else
            {
                return (ushort)(ReadBits(8) | (ReadBits(bits - 8) << 8));
            }
        }

        public int ReadInt(int bits = sizeof(int) * 8)
        {
            return (int)ReadUInt(bits);
        }

        public uint ReadUInt(int bits = sizeof(int) * 8)
        {
            if (bits <= 8)
            {
                return (uint)ReadBits(bits);
            }
            else if (bits <= 16)
            {
                return (uint)(ReadBits(8) | (ReadBits(bits - 8) << 8));
            }
            else if (bits <= 24)
            {
                return (uint)(ReadBits(8) | (ReadBits(8) << 8) | (ReadBits(bits - 16) << 16));
            }
            else
            {
                return (uint)(ReadBits(8) | (ReadBits(8) << 8) | (ReadBits(8) << 16) | (ReadBits(bits - 24) << 24));
            }
        }

        public long ReadLong(int bits = sizeof(long) * 8)
        {
            return (long)ReadULong(bits);
        }

        public ulong ReadULong(int bits = sizeof(ulong) * 8)
        {
            if (bits <= 32)
            {
                return ReadUInt(bits);
            }
            else
            {
                ulong first = ReadUInt(32) & 0xFFFFFFFF;
                ulong second = ReadUInt(bits - 32);
                return first | (second << 32);
            }
        }

        public double ReadDouble()
        {
            var byteConverter = default(ByteConverter);
            byteConverter.Byte0 = ReadBits(8);
            byteConverter.Byte1 = ReadBits(8);
            byteConverter.Byte2 = ReadBits(8);
            byteConverter.Byte3 = ReadBits(8);
            byteConverter.Byte4 = ReadBits(8);
            byteConverter.Byte5 = ReadBits(8);
            byteConverter.Byte6 = ReadBits(8);
            byteConverter.Byte7 = ReadBits(8);

            return byteConverter.Double;
        }

        public float ReadFloat()
        {
            var byteConverter = default(ByteConverter);
            byteConverter.Byte0 = ReadBits(8);
            byteConverter.Byte1 = ReadBits(8);
            byteConverter.Byte2 = ReadBits(8);
            byteConverter.Byte3 = ReadBits(8);

            return byteConverter.Float;
        }

        public string ReadString()
        {
            return Encoding.UTF8.GetString(ReadBytes(ReadBits(8)));
        }

        public long GetStreamPosition()
        {
            return _bitsPointer;
        }

        public byte[] GetBuffer()
        {
            return _buffer;
        }

        public void Clear()
        {
            _buffer.FastClear();
        }

        public int GetSize()
        {
            return _bytePointer + 1;
        }

        public byte[] ToByteArray()
        {
            var bytes = new byte[_bytePointer + 1];
            Buffer.BlockCopy(_buffer, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public void ToByteArray(ref byte[] buffer)
        {
            if(buffer.Length < _bytePointer + 1)
                throw new Exception("The buffer is not large enough.");
            
            Buffer.BlockCopy(_buffer, 0, buffer, 0, _bytePointer + 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureBufferSpace(int additionalSpaceInBits)
        {
            var length = _buffer.Length;
            if ((_bytePointer << 3) + _bitsPointer + additionalSpaceInBits > length << 3)
            {
                if(!_isBufferInternallyManaged)
                    throw new Exception("The buffer is not managed by this writer and cannot be resized.");
                var tmpBuffer = _buffer;
                var newBuffer = _pool.Rent((length + (additionalSpaceInBits >> 8) + 1) * GrowingFactor);
                Buffer.BlockCopy(tmpBuffer, 0, newBuffer, 0, tmpBuffer.Length);
                _pool.Release(_buffer);
                _buffer = newBuffer;
            }
        }

        public void Write(byte value, int bits = sizeof(byte) * 8)
        {
            EnsureBufferSpace(bits);
            WriteBits(value, bits);
        }
      
        public void Write(sbyte value, int bits = sizeof(byte) * 8)
        {
            Write((byte)value, bits);
        }

        public void Write(byte[] value)
        {
            EnsureBufferSpace(value.Length * 8);

            if (_bitsPointer == 0)
            {
                Buffer.BlockCopy(value, 0, _buffer, _bytePointer, value.Length);
                _bytePointer += value.Length;
                _currentByte = _buffer[_bytePointer];
                return;
            }

            for (var index = 0; index < value.Length; index++)
                Write(value[index]);
        }

        public void Write(bool value)
        {
            EnsureBufferSpace(1);
            WriteBits((value) ? (byte)1 : (byte)0, 1);
        }

        public void Write(short value, int bits = sizeof(short) * 8)
        {
            Write((ushort)value, bits);
        }

        public void Write(ushort value, int bits = sizeof(short) * 8)
        {
            EnsureBufferSpace(bits);
            if (bits <= 8)
            {
                WriteBits((byte)value, bits);
            }
            else
            {
                WriteBits((byte)value, 8);
                WriteBits((byte)(value >> 8), bits - 8);
            }
        }
        public void Write(int value, int bits = sizeof(int) * 8)
        {
            Write((uint)value, bits);
        }

        public void Write(uint value, int bits = sizeof(uint) * 8)
        {
            EnsureBufferSpace(bits);
            if (bits <= 8)
            {
                WriteBits((byte)value, bits);
            }
            else if (bits <= 16)
            {
                WriteBits((byte)value, 8);
                WriteBits((byte)(value >> 8), bits - 8);
            }
            else if (bits <= 24)
            {
                WriteBits((byte)value, 8);
                WriteBits((byte)(value >> 8), 8);
                WriteBits((byte)(value >> 16), bits - 16);
            }
            else
            {
                WriteBits((byte)value, 8);
                WriteBits((byte)(value >> 8), 8);
                WriteBits((byte)(value >> 16), 8);
                WriteBits((byte)(value >> 24), bits - 24);
            }
        }

        public void Write(long value, int bits = sizeof(long) * 8)
        {
            Write((ulong)value, bits);
        }

        public void Write(ulong value, int bits = sizeof(ulong) * 8)
        {
            EnsureBufferSpace(bits);

            if (bits <= 32)
            {
                Write((uint)value, bits);
            }
            else
            {
                Write((uint)value);
                Write((uint)(value >> 32), bits - 32);
            }
        }

        public void Write(double value)
        {
            EnsureBufferSpace(sizeof(double) * 8);

            var byteConverter = default(ByteConverter);
            byteConverter.Double = value;

            WriteBits(byteConverter.Byte0, 8);
            WriteBits(byteConverter.Byte1, 8);
            WriteBits(byteConverter.Byte2, 8);
            WriteBits(byteConverter.Byte3, 8);
            WriteBits(byteConverter.Byte4, 8);
            WriteBits(byteConverter.Byte5, 8);
            WriteBits(byteConverter.Byte6, 8);
            WriteBits(byteConverter.Byte7, 8);
        }

        public void Write(float value)
        {
            EnsureBufferSpace(sizeof(float) * 8);

            var byteConverter = default(ByteConverter);
            byteConverter.Float = value;

            WriteBits(byteConverter.Byte0, 8);
            WriteBits(byteConverter.Byte1, 8);
            WriteBits(byteConverter.Byte2, 8);
            WriteBits(byteConverter.Byte3, 8);
        }

        public void Write(string value)
        {
            EnsureBufferSpace(sizeof(char) * 8 * (value.Length + 1));

            WriteBits((byte)value.Length, 8);
            Write(Encoding.UTF8.GetBytes(value));
        }
    }
}