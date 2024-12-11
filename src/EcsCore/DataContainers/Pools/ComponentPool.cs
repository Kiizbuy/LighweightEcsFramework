using System;
using EcsCore.MemoryAllocation;
using EcsCore.Serialization;
using EcsCore.Serialization.Resolvers;
using EcsCore.Utils;
// using ISerializableData = EcsCore.Serialization.ISerializableData;

namespace EcsCore.Components.Pool
{
    public interface IComponentPool : ISerializableData, ICopyable<IComponentPool>
    {
        void Clear();
        void ProcessRemoved();
        void Resize(int count);
        bool RemoveByEntityId(uint entityId);
        bool MarkAsRemoved(uint entityId);
        bool Contains(uint entityId);
        int Mask { get; }
    }

    public interface IComponentPool<T> : IComponentPool,
    ICopyable<IComponentPool<T>> where T : struct, IComponentData
    {
        ComponentPool<T>.ArrayEnumerableByRef GetArrayEnumerableByRef();
        ref T Add(uint entityId, T value);
        ref T Get(uint entityId);
        ref T Get(uint entityId, out bool componentValueExist);
        ref T this[int id] { get; }
        int Count { get; }
    }

    public struct ComponentСontainer<T> where T : struct, IComponentData
    {
        public T ComponentData;
        public bool Disabled;
    }

    public class NetworkComponentPool<T> : ComponentPool<T> where T : struct, IComponentData
    {
        private IComponentResolver<T> _componentResolver;

        public NetworkComponentPool()
        {
            _componentResolver = ResolversMap.GetResolver<T>();
        }

        public override void Serialize(ISerializePacker serializePacker)
        {
            var cannotSerialize = _componentResolver == null;
            serializePacker.Write(cannotSerialize);
            if (_componentResolver == null)
            {
                return;
            }

            serializePacker.Write(Count);
            var enumerator = GetArrayEnumerableByRef().GetEnumerator();
            while (enumerator.MoveNext())
            {
                serializePacker.Write(enumerator.CurrentEntityId);
                _componentResolver.Serialize(ref enumerator.Current, serializePacker);
            }
        }

        public override void Deserialize(ISerializePacker serializePacker)
        {
            if (serializePacker.ReadBool())
            {
                return;
            }
            
            if (_componentResolver == null)
            {
                return;
            }
            
            Clear();
            var newCount = serializePacker.ReadInt();
            Resize(newCount);
            
            for (int i = 0; i < newCount; i++)
            {
                var entityId = serializePacker.ReadUInt();
                var newComponent = new T();
                _componentResolver.Deserialize(serializePacker, ref newComponent);
                Add(entityId, newComponent);
            }
        }
    }

    public class ComponentPool<T> : IComponentPool<T> where T : struct, IComponentData
    {
        private ComponentСontainer<T>[] _values = Array.Empty<ComponentСontainer<T>>();
        private uint[] _ids = Array.Empty<uint>();
        
        private T _defaultValue;
        private int _count;

        private readonly IMemoryAllocator<ComponentСontainer<T>> _memoryAllocator;
        private readonly IMemoryAllocator<uint> _idMemoryAllocator;
        
        private static readonly IMemoryAllocator<ComponentСontainer<T>> DefaultMemoryAllocator = new BlockMemoryAllocator<ComponentСontainer<T>>();
        private static readonly IMemoryAllocator<uint> DefaultIdMemoryAllocator = new BlockMemoryAllocator<uint>();
        
        public int Count => _count;


        public ref T this[int id] => ref _values[id].ComponentData;

        public ComponentPool() 
        : this(DefaultMemoryAllocator, DefaultIdMemoryAllocator)
        {
        }
        
        public ComponentPool(IMemoryAllocator<ComponentСontainer<T>> memoryAllocator, IMemoryAllocator<uint> idMemoryAllocator)
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
                var middleIndex = low + ((high - low) >> 1);

                if (_ids[middleIndex].Equals(entityId))
                {
                    if (!_values[middleIndex].Disabled)
                    {
                        index = middleIndex;
                        return true;
                    }
                    else
                    {
                        high = middleIndex - 1;
                    }
                }
                else
                {
                    if (_ids[middleIndex].CompareTo(entityId) < 0)
                    {
                        low = middleIndex + 1;
                    }
                    else
                    {
                        high = middleIndex - 1;
                    }
                }
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
            _values[index].ComponentData = value;
            _ids[index] = entityId;
            
            return ref _values[index].ComponentData;
        }

        public ref T Get(uint entityId, out bool componentValueExist)
        {
            componentValueExist = false;
            if (!BinarySearchByEntityId(entityId, out var index)) 
                return ref _defaultValue;
            
            componentValueExist = true;
            return ref _values[index].ComponentData;
        }
        
        public ref T Get(uint entityId)
        {
            if (BinarySearchByEntityId(entityId, out var index)) 
                return ref _values[index].ComponentData;
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
#if DEBUG
            Array.Resize(ref _values, 0);
            Array.Resize(ref _ids, 0);
#endif
            _count = 0;
        }

        private void RemoveComponentByIndex(int index)
        {
#if DEBUG
            if (index < 0 && index >= Count)
            {
                throw new IndexOutOfRangeException();
            }
#endif
            
            var isNotLastItemRemoved = index != --_count;

            if (isNotLastItemRemoved && _count > 0)
            {
                Array.Copy(_values, index + 1, _values, index, _count - index);
                Array.Copy(_ids, index + 1, _ids, index, _count - index);
            }
        }

        public bool MarkAsRemoved(uint entityId)
        {
            if (BinarySearchByEntityId(entityId, out var index))
            {
#if DEBUG
                if (index < 0 && index >= Count)
                {
                    throw new IndexOutOfRangeException();
                }
#endif
                ref var componentValue = ref _values[index];
                componentValue.Disabled = true;
                return true;
            }

            return false;
        }

        public bool Contains(uint entityId)
        {
            return BinarySearchByEntityId(entityId, out _);
        }

        public int Mask => EcsComponentTypes<T>.TypeBitMask;

        public void Resize(int count)
        {
            if (_values.Length < count)
            {
                _memoryAllocator.Resize(ref _values, count + 1);
                _idMemoryAllocator.Resize(ref _ids, count + 1);                
            }
        }
        
        public void ProcessRemoved()
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                if (_values[i].Disabled)
                    RemoveComponentByIndex(i);
            }
        }

        public ArrayEnumerableByRef GetArrayEnumerableByRef()
        {
            return new ArrayEnumerableByRef(_values, _ids, _count);
        }
        
        public readonly struct ArrayEnumerableByRef
        {
            private readonly ComponentСontainer<T>[] _values;
            private readonly uint[] _ids;
            private readonly int _count;

            public ArrayEnumerableByRef(ComponentСontainer<T>[] values, uint[] ids, int count)
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
            private readonly ComponentСontainer<T>[] _values;
            private readonly uint[] _ids;
            private readonly int _count;
            
            public StructEnumerator(ComponentСontainer<T>[] values, uint[] ids, int count)
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

                    return ref _values[_index].ComponentData;
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
                return MoveNext(ref this);
            }
            
            public static bool MoveNext(ref StructEnumerator enumerator)
            {
                while (enumerator._index++ < enumerator._count)
                {
                    if (enumerator._index < enumerator._count && !enumerator._values[enumerator._index].Disabled)
                    {
                        return true;
                    }
                }

                return false;
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

        public virtual void Serialize(ISerializePacker serializePacker)
        {
            // serialzePacker.Write(_count);
            // var enumerator = GetArrayEnumerableByRef().GetEnumerator();
            // while (enumerator.MoveNext())
            // {
            //     serialzePacker.Write(enumerator.CurrentEntityId);
            //     enumerator.Current.Serialize(serialzePacker);
            // }
        }

        public virtual void Deserialize(ISerializePacker serializePacker)
        {
            // Clear();
            // var newCount = serialzePacker.ReadInt();
            // Resize(newCount);
            // for (int i = 0; i < newCount; i++)
            // {
            //     var entityId = serialzePacker.ReadUInt();
            //     var newData = new T();
            //     newData.Deserialize(serialzePacker);
            //     Add(entityId, newData);
            // }
        }

        public void CopyTo(IComponentPool other)
        {
            if (other is IComponentPool<T> componentPool)
            {
                //TODO copy to
            }
        }
    }
}