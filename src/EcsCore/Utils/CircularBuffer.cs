namespace EcsCore
{
    public sealed class CircularBuffer<T>
    {
        private int _lastIndex;
        
        public void SetBuffer(T[] data)
        {
            GetBuffer = data;
            _lastIndex = data.Length - 1;
        }

        public ref T Current => ref GetBuffer[_lastIndex];

        public T[] GetBuffer { get; private set; }

        public ref T GetNext()
        {
            ++_lastIndex;
            if (_lastIndex > GetBuffer.Length - 1)
            {
                _lastIndex = 0;
            }

            return ref GetBuffer[_lastIndex];
        }

        public int GetNextIndex()
        {
            var num = _lastIndex + 1;
            if (num >= GetBuffer.Length)
                num = 0;
            return num;
        }
    }
}