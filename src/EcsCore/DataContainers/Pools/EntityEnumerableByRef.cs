using EcsCore.Containers;

namespace EcsCore
{
    public readonly struct EntityEnumerableByRef<TValue> where TValue : struct, IPollableData
    {
        private readonly DataPool<uint, TValue>.ArrayEnumerableByRef _inner;

        public EntityEnumerableByRef(DataPool<uint, TValue>.ArrayEnumerableByRef inner)
        {
            _inner = inner;
        }

        public Enumerator GetEnumerator() => new(_inner.GetEnumerator());

        public struct Enumerator
        {
            private DataPool<uint, TValue>.ArrayEnumerableByRef.Enumerator _inner;

            public Enumerator(DataPool<uint, TValue>.ArrayEnumerableByRef.Enumerator inner)
            {
                _inner = inner;
            }

            public int GetIndex => _inner.GetIndex;
            public int Count => _inner.Count;
            public ref TValue Current => ref _inner.Current;
            public uint CurrentEntityId => _inner.CurrentKey;
            public bool MoveNext() => DataPool<uint, TValue>.ArrayEnumerableByRef.Enumerator.MoveNext(ref _inner);
            public void Reset() => DataPool<uint, TValue>.ArrayEnumerableByRef.Enumerator.Reset(ref _inner);
        }
    }
}