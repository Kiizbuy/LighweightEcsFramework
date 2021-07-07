using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EcsCore.Components;
using EcsCore.Components.Pool;
using EcsCore.Utils;
using NetCodeUtils;

namespace EcsCore
{
    //TODO Add pool component generation for all types
    public sealed class EcsState : ISerializableData, ICopyable<EcsState>
    {
        public uint Tick = 0;

        private readonly IDictionary<uint, Entity> _entitiesMap = new Dictionary<uint, Entity>();
        private readonly IDictionary<int, EntitySpace> _entitySpaces = new Dictionary<int, EntitySpace>();
        private uint _entitiesPerSpace = 1000;
        private int _poolsLength = 31;

        private IComponentPool[] _pools = new IComponentPool[32];

        private EntitySpace GetEntitySpace(int entitySpaceId)
        {
            if (_entitySpaces.TryGetValue(entitySpaceId, out var entitySpace))
                return entitySpace;

            entitySpace = new EntitySpace((uint) (entitySpaceId * _entitiesPerSpace), _entitiesPerSpace);

            return _entitySpaces[entitySpaceId] = entitySpace;
        }

        public EcsState SetupEntitiesPerSpace(uint entitiesPerSpace)
        {
            _entitiesPerSpace = entitiesPerSpace;
            return this;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IComponentPool<T> GetPool<T>() where T : struct, IComponentData, ISerializableData
        {
            var typeIndex = EcsPoolTypes<T>.TypeIndex;
            if (_poolsLength < typeIndex)
            {
                var poolsLength = _poolsLength << 1;
                while (poolsLength <= typeIndex)
                {
                    poolsLength <<= 1;
                }

                Array.Resize(ref _pools, poolsLength);
                _poolsLength = poolsLength;
            }

            var pool = (IComponentPool<T>) _pools[typeIndex];

            if (pool != null)
                return pool;

            pool = new ComponentPool<T>();
            _pools[typeIndex] = pool;
            return pool;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetEntitySpaceByEntityId(uint id)
        {
            return (int) (id / _entitiesPerSpace);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetOwnerId(uint entityId)
        {
            return (int) (entityId / _entitiesPerSpace);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Entity CreateEntity(int entitySpaceId = 0)
        {
            var newEntityId = GetEntitySpace(entitySpaceId).GetNewEntityId();
            var ent = new Entity(this, newEntityId);
            return _entitiesMap[newEntityId] = ent;
        }

        public void DestroyEntity(uint entityId)
        {
            _entitySpaces[GetEntitySpaceByEntityId(entityId)].ReleaseEntityId(entityId);
            _entitiesMap.Remove(entityId);
        }

        public void DestroyAllEntities()
        {
            _entitiesMap.Clear();
        }

        public void CopyTo(EcsState other)
        {
            other.SetupEntitiesPerSpace(_entitiesPerSpace);

            other._entitiesMap.Clear();
            foreach (var space in _entitySpaces)
            {
                var newSpace = new EntitySpace(space.Value.Base, space.Value.Size);
                space.Value.CopyTo(newSpace);
                other._entitySpaces.Add(space.Key, newSpace);
            }

            other._entitiesMap.Clear();
            foreach (var entity in _entitiesMap)
            {
                other._entitiesMap.Add(entity.Key, entity.Value);
            }

            other._poolsLength = _poolsLength;
            other._entitiesPerSpace = _entitiesPerSpace;
            Array.Copy(_pools, 0, other._pools, 0, _poolsLength);
        }

        //TODO Add more fast filter without boxing/unboxing
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<Entity> GetFilteredEntities<T>() where T : struct, IComponentData, ISerializableData
        {
            var pool = GetPool<T>().GetArrayEnumerableByRef().GetEnumerator();

            while (pool.MoveNext())
                yield return _entitiesMap[pool.CurrentEntityId];
        }

        public void Clear()
        {
            _entitiesMap.Clear();
            _entitySpaces.Clear();
            for (int i = 0; i < _pools.Length; i++)
            {
                _pools[i]?.Clear();
            }
        }

        public void Serialize(IPacker packer)
        {
            packer.WriteInt(_poolsLength);
            for (int i = 0; i < _poolsLength; i++)
            {
                packer.WriteBool(_pools[i] != null);
                if (_pools[i] != null)
                    _pools[i].Serialize(packer);
            }
            packer.WriteInt(_entitiesMap.Values.Count);
            foreach (var entityid in _entitiesMap.Keys)
            {
                packer.WriteUint(entityid);
            }
        }

        //TODO all PoolsTypes when we init EcsState
        public void Deserialize(IPacker packer)
        {
            _poolsLength = packer.ReadInt();
            for (int i = 0; i < _poolsLength; i++)
            {
                if (!packer.ReadBool())
                    continue;
                _pools[i].Deserialize(packer);
            }
            _entitiesMap.Clear();
            var entitiesCount = packer.ReadInt();
            for (int i = 0; i < entitiesCount; i++)
            {
                var entId = packer.ReadUint();
                _entitiesMap.Add(entId, new Entity(this, entId));
            }            
        }
    }
}