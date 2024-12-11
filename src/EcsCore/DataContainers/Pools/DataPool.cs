using System;
using EcsCore.MemoryAllocation;

namespace EcsCore.Containers
{
    public class DataPool<TKey, TValue>
        where TKey : struct, IEquatable<TKey>, IComparable<TKey>
        where TValue : struct, IPollableData
    {
        protected static readonly TKey[] EmptyKeys = Array.Empty<TKey>();
        protected static readonly TValue[] EmptyValues = Array.Empty<TValue>();

        protected readonly IMemoryAllocator<TValue> _valuesMemoryAllocator;
        protected readonly IMemoryAllocator<TKey> _keysMemoryAllocator;

        protected TValue[] _items = EmptyValues;
        protected TKey[] _keys = EmptyKeys;

        private TValue DefaultValue = new TValue();

        public ref TValue this[int i] => ref _items[i];

        public int Count { get; protected set; }

        public DataPool()
            : this(DefaultValuesMemoryAllocator, DefaultKeysContainerMemoryAllocator)
        {
        }

        public DataPool(IMemoryAllocator<TValue> valuesMemoryAllocator)
        {
            _valuesMemoryAllocator = valuesMemoryAllocator;
        }

        public DataPool(IMemoryAllocator<TValue> valuesMemoryAllocator, IMemoryAllocator<TKey> keysMemoryAllocator)
            : this(valuesMemoryAllocator)
        {
            _keysMemoryAllocator = keysMemoryAllocator;
        }

        public void Resize(int count)
        {
            if (_items.Length < count)
                _valuesMemoryAllocator.Resize(ref _items, count + 1);

            if (_keys.Length < count)
                _keysMemoryAllocator.Resize(ref _keys, count + 1);
        }

        public ref TValue GetByKey(TKey key)
        {
            if (BinarySearchByKey(key, out var index))
            {
                return ref _items[index];
            }
#if DEBUG
            throw new NullReferenceException($"Key {key} has no value of type {typeof(TValue).Name}");
#endif
            return ref DefaultValue;
        }

        public ref TValue GetByKey(TKey key, out bool present)
        {
            if (BinarySearchByKey(key, out var index))
            {
                present = true;
                return ref _items[index];
            }

            present = false;
            return ref DefaultValue;
        }

        public bool RemoveByKey(TKey key)
        {
            if (BinarySearchByKey(key, out var index))
            {
                Remove(index);
                return true;
            }

            return false;
        }

        private static bool BinarySearchByKey(TKey key, TKey[] keys, TValue[] items, int count, out int index)
        {
            var low = 0;
            var high = count - 1;

            while (low <= high)
            {
                var i = low + ((high - low) >> 1);

                if (keys[i].Equals(key))
                {
                    if (!items[i].Disabled)
                    {
                        index = i;
                        return true;
                    }
                    else
                    {
                        high = i - 1;
                    }
                }
                else
                {
                    if (keys[i].CompareTo(key) < 0)
                    {
                        low = i + 1;
                    }
                    else
                    {
                        high = i - 1;
                    }
                }
            }

            index = low;
            return false;
        }

        private bool BinarySearchByKey(TKey key, out int index)
        {
            return BinarySearchByKey(key, _keys, _items, Count, out index);
        }

        public ref TValue Add(TKey key)
        {
            var noFreeItems = Count == _items.Length;

            if (noFreeItems)
            {
                _valuesMemoryAllocator.Resize(ref _items, _items.Length + 1);
                _defaultKeysContainerMemoryAllocator.Resize(ref _keys, _keys.Length + 1);
            }

            if (BinarySearchByKey(key, out var index))
                throw new NotSupportedException($"Only one instance of value per key supported {key}");

            if (_items.Length - 1 != index)
            {
                Array.Copy(_items, index, _items, index + 1, Count - index);
                Array.Copy(_keys, index, _keys, index + 1, Count - index);
            }

            Count++;
            _keys[index] = key;

            return ref _items[index];
        }

        public ref TValue Add(TKey key, TValue value)
        {
            var noFreeItems = Count == _items.Length;

            if (noFreeItems)
            {
                _valuesMemoryAllocator.Resize(ref _items, _items.Length + 1);
                _defaultKeysContainerMemoryAllocator.Resize(ref _keys,
                    _keys.Length + 1);
            }

            if (BinarySearchByKey(key, out var index))
                throw new NotSupportedException($"Only one instance of value per key supported {key}");

            if (_items.Length - 1 != index)
            {
                Array.Copy(_items, index, _items, index + 1, Count - index);
                Array.Copy(_keys, index, _keys, index + 1, Count - index);
            }

            Count++;
            _keys[index] = key;
            _items[index] = value;
            return ref _items[index];
        }

        public void Remove(int i)
        {
#if DEBUG
            if (i < 0 && i >= Count)
            {
                throw new IndexOutOfRangeException();
            }
#endif
            var isNotLastItemRemoved = i != --Count;

            if (isNotLastItemRemoved && Count > 0)
            {
                Array.Copy(_items, i + 1, _items, i, Count - i);
                Array.Copy(_keys, i + 1, _keys, i, Count - i);
            }
        }

        public bool Contains(ref TValue item)
        {
            return Array.IndexOf(_items, item) != -1;
        }

        public bool ContainsKey(TKey key)
        {
            return Array.IndexOf(_keys, key) != -1;
        }

        public int IndexOf(ref TValue item)
        {
            return Array.IndexOf(_items, item);
        }

        public void Clear()
        {
#if DEBUG
            Array.Clear(_items, 0, Count);
            Array.Clear(_keys, 0, Count);
#endif
            Count = 0;
        }

        public void CopyTo(DataPool<TKey, TValue> other)
        {
            other.Resize(this.Count);

            Array.Copy(this._items, other._items, this.Count);
            Array.Copy(this._keys, other._keys, this.Count);

            other.Count = this.Count;
        }

        private static IMemoryAllocator<TValue> _defaultValuesMemoryAllocator;

        private static IMemoryAllocator<TValue> DefaultValuesMemoryAllocator =>
            _defaultValuesMemoryAllocator ??= new BlockMemoryAllocator<TValue>();

        private static IMemoryAllocator<TKey> _defaultKeysContainerMemoryAllocator;

        private static IMemoryAllocator<TKey> DefaultKeysContainerMemoryAllocator =>
            _defaultKeysContainerMemoryAllocator ??= new BlockMemoryAllocator<TKey>();

        public static class ArrayExtensions
        {
            public static ArrayEnumerableByRef ToEnumerableByRef(DataPool<TKey, TValue> array)
            {
                return new ArrayEnumerableByRef(array._items, array._keys, array.Count);
            }
        }

        public readonly struct ArrayEnumerableByRef
        {
            private readonly TValue[] _target;
            private readonly TKey[] _keysContainer;
            private readonly int _count;

            public ArrayEnumerableByRef(TValue[] target, TKey[] keysContainer, int count)
            {
                _target = target;
                _keysContainer = keysContainer;
                _count = count;
            }

            public Enumerator GetEnumerator() => new(_target, _keysContainer, _count);

            public struct Enumerator
            {
                private readonly TValue[] _target;
                private readonly TKey[] _ids;
                private readonly int _count;
                private int _index;

                public Enumerator(TValue[] target, TKey[] ids, int count)
                {
                    _target = target;
                    _ids = ids;
                    _count = count;
                    _index = -1;
                }

                public int GetIndex => _index;
                public int Count => _count;

                public ref TValue Current
                {
                    get
                    {
                        if (_target is null || _index < 0 || _index > _count)
                        {
                            throw new InvalidOperationException(
                                $"_target is null: {_target is null}, index: {_index}, count: {_count}");
                        }

                        return ref _target[_index];
                    }
                }

                public TKey CurrentKey
                {
                    get
                    {
                        if (_ids is null || _index < 0 || _index > _count)
                        {
                            throw new InvalidOperationException(
                                $"_ids is null: {_ids is null}, index: {_index}, count: {_count}");
                        }

                        return _ids[_index];
                    }
                }

                public bool MoveNext() => MoveNext(ref this);

                public static bool MoveNext(ref Enumerator enumerator)
                {
                    while (enumerator._index++ < enumerator._count)
                    {
                        if (enumerator._index < enumerator._count && !enumerator._target[enumerator._index].Disabled)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                public static void Reset(ref Enumerator enumerator) => enumerator._index = -1;

                public void Reset() => Reset(ref this);
            }
        }

        public bool MarkAsRemoved(TKey key)
        {
            if (BinarySearchByKey(key, out var index))
            {
#if DEBUG
                if (index < 0 && index >= Count)
                {
                    throw new IndexOutOfRangeException();
                }
#endif
                _items[index].Disabled = true;
                return true;
            }

            return false;
        }

        public void ProcessRemoved()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                if (_items[i].Disabled)
                    Remove(i);
            }
        }
    }
}