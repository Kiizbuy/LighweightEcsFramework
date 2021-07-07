using System;
using System.Runtime.CompilerServices;
using EcsCore.MemoryAllocation;
using EcsCore.Utils;
using NetCodeUtils;

namespace EcsCore.Components.Pool
{
    public interface IComponentPool : ISerializableData
    {
        void Clear();
        void Resize(int count);
        bool RemoveByEntityId(uint entityId);
    }

    public interface IComponentPool<T> : IComponentPool, ICopyable<IComponentPool<T>> where T : struct, IComponentData, ISerializableData
    {
        ComponentPool<T>.ArrayEnumerableByRef GetArrayEnumerableByRef();
        ref T Add(uint entityId, T value);
        ref T Get(uint entityId);
        ref T Get(uint entityId, out bool componentValueExist);
        ref T this[int id] { get; }
        int Count { get; }
    }
    
    public sealed class ComponentPool<T> : IComponentPool<T> where T : struct, IComponentData, ISerializableData
    {
        private T[] _values = Array.Empty<T>();
        private uint[] _ids = Array.Empty<uint>();
        private T _defaultValue;
        private int _count;

        private readonly IMemoryAllocator<T> _memoryAllocator;
        private readonly IMemoryAllocator<uint> _idMemoryAllocator;
        
        private static readonly IMemoryAllocator<T> DefaultMemoryAllocator = new BlockMemoryAllocator<T>();
        private static readonly IMemoryAllocator<uint> DefaultIdMemoryAllocator = new BlockMemoryAllocator<uint>();
        
        public int Count => _count;
        public ref T this[int id] => ref _values[id];

        public ComponentPool() 
        : this(DefaultMemoryAllocator, DefaultIdMemoryAllocator)
        {
        }
        
        public ComponentPool(IMemoryAllocator<T> memoryAllocator, IMemoryAllocator<uint> idMemoryAllocator)
        {
            _memoryAllocator = memoryAllocator;
            _idMemoryAllocator = idMemoryAllocator;
        }

        private bool BinarySearchByEntityId(uint entityId, out int index)
        {
            var low = 0;
            var high = _count - 1;

            while (low <= high)
            {
                var middle = (low + high) / 2;

                if (entityId == _ids[middle])
                {
                    index = middle;
                    return true;
                }

                if (entityId < _ids[middle])
                    high = middle - 1;
                else
                    low = middle + 1;
            }

            index = low;
            return false;
        }

        public ref T Add(uint entityId, T value)
        {
            var noFreeItems = _count == _values.Length;
            if (noFreeItems)
            {
                _memoryAllocator.Resize(ref _values, _values.Length + 1);
                _idMemoryAllocator.Resize(ref _ids, _ids.Length + 1);
            }

            if (BinarySearchByEntityId(entityId, out var index))
                throw new NotSupportedException($"Only one instance of component per entity supported {entityId}");

            if (_values.Length - 1 != index)
            {
                Array.Copy(_values, index, _values, index + 1, _count - index);
                Array.Copy(_ids, index, _ids, index + 1, _count - index);
            }

            _count++;
            _values[index] = value;
            _ids[index] = entityId;
            
            return ref _values[index];
        }

        public ref T Get(uint entityId, out bool componentValueExist)
        {
            componentValueExist = false;
            if (!BinarySearchByEntityId(entityId, out var index)) 
                return ref _defaultValue;
            
            componentValueExist = true;
            return ref _values[index];
        }
        
        public ref T Get(uint entityId)
        {
            if (BinarySearchByEntityId(entityId, out var index)) 
                return ref _values[index];
#if DEBUG
            throw new NullReferenceException($"Component doesnt exist on entity id{entityId}");
#endif
            return ref _defaultValue;

        }

        public bool RemoveByEntityId(uint entityId)
        {
            if (!BinarySearchByEntityId(entityId, out var index))
            {
#if DEBUG
                throw new NullReferenceException($"entity with id{entityId} doesnt have component {nameof(T)}");
#endif
                return false;
            }

            RemoveComponentByIndex(index);
            return true;
        }

        public void Clear()
        {
            Array.Resize(ref _values, 0);
            Array.Resize(ref _ids, 0);
            _count = 0;
        }

        private void RemoveComponentByIndex(int index)
        {
            var isNotLastItemRemoved = index != --_count;

            if (isNotLastItemRemoved && _count > 0)
            {
                Array.Copy(_values, index + 1, _values, index, _count - index);
                Array.Copy(_ids, index + 1, _ids, index, _count - index);
            }
        }

        public void Resize(int count)
        {
            if (_values.Length < count)
            {
                _memoryAllocator.Resize(ref _values, count + 1);
                _idMemoryAllocator.Resize(ref _ids, count + 1);                
            }
        }

        public ArrayEnumerableByRef GetArrayEnumerableByRef()
        {
            return new ArrayEnumerableByRef(_values, _ids, _count);
        }
        
        public readonly struct ArrayEnumerableByRef
        {
            private readonly T[] _values;
            private readonly uint[] _ids;
            private readonly int _count;

            public ArrayEnumerableByRef(T[] values, uint[] ids, int count)
            {
                _values = values;
                _ids = ids;
                _count = count;
            }

            public int Count => _count;
            
            public StructEnumerator GetEnumerator()
            {
                return new StructEnumerator(_values, _ids, _count);
            }

        }
        
        public struct StructEnumerator
        {
            private int _index;
            private readonly T[] _values;
            private readonly uint[] _ids;
            private readonly int _count;
            
            public StructEnumerator(T[] values, uint[] ids, int count)
            {
                _values = values;
                _ids = ids;
                _index = -1;
                _count = count;
            }
            
            public ref T Current
            {
                get
                {
                    if (_values is null || _index < 0 || _index > _count)
                    {
                        throw new InvalidOperationException(
                            $"_target is null: {_values is null}, index: {_index}, count: {_count}");
                    }

                    return ref _values[_index];
                }
            }

            public uint CurrentEntityId
            {
                get
                {
                    if (_ids is null || _index < 0 || _index > _count)
                    {
                        throw new InvalidOperationException(
                            $"_target is null: {_values is null}, index: {_index}, count: {_count}");
                    }

                    return _ids[_index];
                }
            }
            

            public bool MoveNext()
            {
                return ++_index < _count;
            }

            public void Reset()
            {
                _index = -1;
            }
        }

        public void CopyTo(IComponentPool<T> other)
        {
            var otherPool = (ComponentPool<T>) other;
            otherPool._count = _count;
            
            Array.Copy(_ids, 0, otherPool._ids, 0, _count);
            Array.Copy(_values, 0, otherPool._values, 0, _count);
        }

        public void Serialize(IPacker packer)
        {
            packer.WriteInt(_count);
            var enumerator = GetArrayEnumerableByRef().GetEnumerator();
            while (enumerator.MoveNext())
            {
                packer.WriteUint(enumerator.CurrentEntityId);
                enumerator.Current.Serialize(packer);
            }
        }

        public void Deserialize(IPacker packer)
        {
            Clear();
            var newCount = packer.ReadInt();
            Resize(newCount);
            for (int i = 0; i < newCount; i++)
            {
                var entityId = packer.ReadUint();
                var newData = new T();
                newData.Deserialize(packer);
                Add(entityId, newData);
            }
        }
    }
}