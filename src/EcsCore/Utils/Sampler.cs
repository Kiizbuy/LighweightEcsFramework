using System;

namespace EcsCore
{
    public class Sampler<TData> where TData : new()
    {
        private Sample<TData>[] _buffer;
        private int _id;

        public Sample<TData>[] GetSamples() => _buffer;
        public int Id => _id;

        public void Init(int bufferSize, int id)
        {
            _id = id;
            _buffer = new Sample<TData>[bufferSize];
           
            for (var index = 0; index < bufferSize; ++index)
                _buffer[index] = new Sample<TData>();
        }

        public Sample<TData> CreateSample(uint tick)
        {
            var sample = GetSample(tick);
            
            if (sample != null)
                return sample;
            
            var index1 = -1;
            var num = uint.MaxValue;
            for (var index2 = 0; index2 < _buffer.Length; ++index2)
            {
                if (num > _buffer[index2].Tick)
                {
                    num = _buffer[index2].Tick;
                    index1 = index2;
                }
            }

            _buffer[index1].Tick = tick;
            return _buffer[index1];
        }

        public Sample<TData> GetSample(uint tick)
        {
            if (_buffer == null)
                return null;
            for (var index = 0; index < _buffer.Length; ++index)
            {
                if ((int)tick == (int)_buffer[index].Tick)
                    return _buffer[index];
            }

            return null;
        }

        public Sample<TData> GetClosestSample(uint tick)
        {
            if (_buffer == null)
                return null;
            var simulationTick = _buffer[0].Tick;
            var index1 = 0;
            for (var index2 = 1; index2 < _buffer.Length; ++index2)
            {
                if (Math.Abs(tick - simulationTick) >
                    Math.Abs(tick - _buffer[index2].Tick))
                {
                    simulationTick = _buffer[index2].Tick;
                    index1 = index2;
                }
            }

            return _buffer[index1];
        }

        public uint GetMaxTick()
        {
            uint num = 0;
            for (var index = 0; index < _buffer.Length; ++index)
            {
                if (_buffer[index].Tick > num)
                    num = _buffer[index].Tick;
            }

            return num;
        }

        public uint GetMinTick()
        {
            var num = uint.MaxValue;
            for (var index = 0; index < _buffer.Length; ++index)
            {
                if (_buffer[index].Tick < num)
                    num = _buffer[index].Tick;
            }

            return num;
        }

        public void Clear()
        {
            for (var index = 0; index < _buffer.Length; ++index)
                _buffer[index].Tick = 0U;
        }
    }

    public class Sample<TDataType> where TDataType : new()
    {
        public TDataType Data = new();
        public uint Tick;
    }
}