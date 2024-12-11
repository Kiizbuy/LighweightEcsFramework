using System;
using System.Runtime.CompilerServices;
using EcsCore.Components;

namespace EcsCore
{
    public struct Entity : IEquatable<Entity>, IPollableData
    {
        internal int AddedComponentsMask;
        internal int AddedViewComponentsMask;

        private EcsState _ownerState;
        private int _index;
        private uint _entityId;
        private bool _disabled;

        internal Entity(EcsState ownerState, uint entityId, int index)
        {
            AddedComponentsMask = 0;
            AddedViewComponentsMask = 0;
            _ownerState = ownerState;
            _entityId = entityId;
            _index = index;
            _disabled = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ChangeStateOwner(EcsState newOwner)
        {
            _ownerState = newOwner;
        }

        public int OwnerId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _ownerState.GetOwnerId(_entityId);
        }

        public uint Id
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entityId;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>() where T : struct, IComponentData
        {
            var typeIndex = EcsComponentTypes<T>.TypeBitMask;
            ref var own = ref _ownerState.GetEntityDataPool[_index];
            own.AddedComponentsMask |= typeIndex;
            return ref _ownerState.GetPool<T>().Add(_entityId, new T());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T AddComponent<T>(T value) where T : struct, IComponentData
        {
            var typeIndex = EcsComponentTypes<T>.TypeBitMask;
            ref var own = ref _ownerState.GetEntityDataPool[_index];
            own.AddedComponentsMask |= typeIndex;
            return ref _ownerState.GetPool<T>().Add(_entityId, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool RemoveComponent<T>() where T : struct, IComponentData
        {
            var typeBitMask = EcsComponentTypes<T>.TypeBitMask;
            ref var own = ref _ownerState.GetEntityDataPool[_index];
            own.AddedComponentsMask &= ~typeBitMask;
            return _ownerState.GetPool<T>().MarkAsRemoved(_entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetComponent<T>() where T : struct, IComponentData
        {
            return ref _ownerState.GetPool<T>().Get(_entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent<T>() where T : struct, IComponentData
        {
            var typeBit = EcsComponentTypes<T>.TypeBitMask;
            return (AddedComponentsMask & typeBit) == typeBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasNoComponents()
        {
            return AddedComponentsMask == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Entity other)
        {
            return _entityId == other._entityId
                   && AddedComponentsMask == other.AddedComponentsMask
                   && OwnerId == other.OwnerId
                   && _index == other._index
                   && _disabled == other._disabled;
        }

        bool IPollableData.Disabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _disabled;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _disabled = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Entity a, Entity b)
        {
            return a.Equals(b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Entity a, Entity b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return obj is Entity other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = AddedComponentsMask;
                hashCode = (hashCode * 397) ^ _index;
                hashCode = (hashCode * 397) ^ (int)_entityId;
                hashCode = (hashCode * 397) ^ _disabled.GetHashCode();
                return hashCode;
            }
        }
    }
}