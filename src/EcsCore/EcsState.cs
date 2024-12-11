using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EcsCore.Components;
using EcsCore.Components.Pool;
using EcsCore.Containers;
using EcsCore.Serialization;
using EcsCore.Utils;

namespace EcsCore
{
    public sealed class EcsState : ISerializableData, ICopyable<EcsState>, IMergable<EcsState>
    {
        public uint LocalTick = 0;
        public uint SimTick = 0;

        private readonly DataPool<uint, Entity> _entitiesData = new();
        private readonly IDictionary<int, EntitySpace> _entitySpaces = new Dictionary<int, EntitySpace>();
        private uint _entitiesPerSpace = 1000;
        private int _poolsLength = 31;
        private bool _locked;

        private IComponentPool[] _pools = new IComponentPool[32];
        private IDictionary<int, IComponentPool> _componentHashToPool = new Dictionary<int, IComponentPool>();

        private EntitySpace GetEntitySpace(int entitySpaceId)
        {
            if (_entitySpaces.TryGetValue(entitySpaceId, out var entitySpace))
                return entitySpace;

            entitySpace = new EntitySpace((uint)(entitySpaceId * _entitiesPerSpace), _entitiesPerSpace);

            return _entitySpaces[entitySpaceId] = entitySpace;
        }

        private EntitySpace GetEntitySpace(uint entityId)
        {
            var entitySpaceId = (int)(entityId / _entitiesPerSpace);
            return GetEntitySpace(entitySpaceId);
        }

        public EcsState SetupEntitiesPerSpace(uint entitiesPerSpace)
        {
            _entitiesPerSpace = entitiesPerSpace;
            return this;
        }
#if DEBUG
        internal void Lock()
        {
            _locked = true;
        }

        internal void Unlock()
        {
            _locked = false;
        }
#endif
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal IComponentPool<T> GetPool<T>() where T : struct, IComponentData
        {
            var typeHash = EcsComponentTypes<T>.TypeHash;
            // if (_poolsLength < typeIndex)
            // {
            //     ResizeComponentPools(typeIndex);
            // }


            // var pool = (IComponentPool<T>)_pools[typeIndex];
            var pool = (IComponentPool<T>)_componentHashToPool[typeHash];

            if (pool != null)
                return pool;

            pool = new NetworkComponentPool<T>();
            _componentHashToPool.Add(typeHash, pool);
            // _pools[typeIndex] = pool;
            return pool;
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // internal IComponentPool<T> GetPool<T>() where T : struct, IComponentData
        // {
        //     var typeIndex = EcsComponentTypes<T>.TypeIndex;
        //     if (_poolsLength < typeIndex)
        //     {
        //         ResizeComponentPools(typeIndex);
        //     }
        //
        //     var pool = (IComponentPool<T>)_pools[typeIndex];
        //
        //     if (pool != null)
        //         return pool;
        //
        //     pool = new NetworkComponentPool<T>();
        //     _pools[typeIndex] = pool;
        //     return pool;
        // }

        private void ResizeComponentPools(int newSize)
        {
            var poolsLength = _poolsLength << 1;
            while (poolsLength <= newSize)
            {
                poolsLength <<= 1;
            }

            Array.Resize(ref _pools, poolsLength);
            _poolsLength = poolsLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EntityExist(in Entity entity)
        {
            return GetEntitySpace(entity.Id).IsSpaceEntityId(entity.Id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool EntityExist(in uint entityId)
        {
            return GetEntitySpace(entityId).IsSpaceEntityId(entityId);
        }

        internal DataPool<uint, Entity> GetEntityDataPool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _entitiesData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityEnumerableByRef<Entity> GetAllEntitiesByRef()
        {
            return new EntityEnumerableByRef<Entity>(DataPool<uint, Entity>.ArrayExtensions.ToEnumerableByRef(_entitiesData));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Entity GetEntityById(uint id)
        {
            return ref _entitiesData.GetByKey(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetEntitySpaceByEntityId(uint id)
        {
            return (int)(id / _entitiesPerSpace);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetOwnerId(uint entityId)
        {
            return (int)(entityId / _entitiesPerSpace);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Entity CreateEntity(int entitySpaceId = 0)
        {
#if DEBUG
            if (_locked) 
            {
                throw new InvalidOperationException("StateIsLocked");
            }
#endif
            var entitySpace = GetEntitySpace(entitySpaceId);
            var newEntityId = entitySpace.GetNewEntityId();
    
#if DEBUG
            if (_entitiesData.ContainsKey(newEntityId))
            {
                entitySpace.ReleaseEntityId(newEntityId);
                throw new Exception(string.Format("EntityIsAlreadyExistsMessage. entity id {0}", newEntityId));
            }
           
#endif
            var newEntity = new Entity(this, newEntityId, _entitiesData.Count);
            return ref _entitiesData.Add(newEntity.Id, newEntity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref Entity GetEntity(uint id)
        {
            return ref _entitiesData.GetByKey(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyEntity(uint entityId)
        {
#if DEBUG
            if (_locked)
            {
                throw new Exception(string.Format("StateIsLocked. cannot destroy entity with id {0}", entityId));
            }
#endif
            _entitySpaces[GetEntitySpaceByEntityId(entityId)].ReleaseEntityId(entityId);
            _entitiesData.MarkAsRemoved(entityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyEntity(in Entity entity)
        {
            DestroyEntity(entity.Id);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DestroyAllEntities()
        {
#if DEBUG
            if (_locked)
            {
                throw new Exception("StateIsLocked. cannot destroy all entities");
            }
#endif
            foreach (var entity in new EntityEnumerableByRef<Entity>(DataPool<uint, Entity>.ArrayExtensions.ToEnumerableByRef(_entitiesData)))
            {
                _entitySpaces[GetEntitySpaceByEntityId(entity.Id)].ReleaseEntityId(entity.Id);
            }
            
            _entitiesData.Clear();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MergeEntitiesTo(DataPool<uint, Entity> to)
        {
            var el = DataPool<uint, Entity>.ArrayExtensions.ToEnumerableByRef(_entitiesData).GetEnumerator();
            while (el.MoveNext())
            {
                ref var newE1 = ref to.Add(el.CurrentKey);
                newE1 = el.Current;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ProcessRemoved()
        {
            var entityRefEnumerable =
                new EntityEnumerableByRef<Entity>(DataPool<uint, Entity>.ArrayExtensions.ToEnumerableByRef(_entitiesData));
            foreach (ref var entity in entityRefEnumerable)
            {
                var entityId = entity.Id;
                var entitySpaceId = GetEntitySpaceByEntityId(entityId);
                if (_entitySpaces.TryGetValue(entitySpaceId, out var entitySpace))
                {
                    entitySpace.ReleaseEntityId(entityId);
                }
            }

            _entitiesData.ProcessRemoved();
        }
      
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(EcsState other)
        {
            ProcessRemoved();
            other.ProcessRemoved();
            other.SetupEntitiesPerSpace(_entitiesPerSpace);
            other._entitiesData.Clear();

            foreach (var space in _entitySpaces)
            {
                var newSpace = new EntitySpace(space.Value.Base, space.Value.Size);
                space.Value.CopyTo(newSpace);
                other._entitySpaces.Add(space.Key, newSpace);
            }

            _entitiesData.CopyTo(other._entitiesData);

            other._poolsLength = _poolsLength;
            other._entitiesPerSpace = _entitiesPerSpace;
            // Array.Copy(_pools, 0, other._pools, 0, _poolsLength);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _entitiesData.Clear();
            _entitySpaces.Clear();
            // foreach (var pool in _pools)
            // {
            //     pool?.Clear();
            // }

            foreach (var pool in _componentHashToPool.Values)
            {
                pool.Clear();
            }
        }

        public void Serialize(ISerializePacker serializePacker)
        {
            ProcessRemoved();
            SerializeComponentPools(serializePacker);
            SerializeEntityPool(serializePacker);
        }

        private void SerializeEntityPool(ISerializePacker serializePacker)
        {
            serializePacker.Write(_entitiesData.Count);
            for (int i = 0; i < _entitiesData.Count; i++)
            {
                serializePacker.Write(_entitiesData[i].Id);
                serializePacker.Write(i);
            }
        }

        private void SerializeComponentPools(ISerializePacker serializePacker)
        {
            foreach (var hashToPool in _componentHashToPool)
            {
                serializePacker.Write(hashToPool.Key);
                hashToPool.Value.Serialize(serializePacker);
            }
            // serializePacker.Write(_poolsLength);
            // for (int i = 0; i < _poolsLength; i++)
            // {
            //     serializePacker.Write(_pools[i] != null);
            //     if (_pools[i] != null)
            //         _pools[i].Serialize(serializePacker);
            // }
        }

        public void Deserialize(ISerializePacker serializePacker)
        {
            Clear();
            DeserializeComponentPools(serializePacker);
            DeserializeEntitiesPool(serializePacker);
        }

        private void DeserializeEntitiesPool(ISerializePacker serializePacker)
        {
            _entitiesData.Clear();
            var entitiesCount = serializePacker.ReadInt();
            for (int i = 0; i < entitiesCount; i++)
            {
                var entId = serializePacker.ReadUInt();
                var index = serializePacker.ReadInt();
                var replicatedEntity = new Entity(this, entId, index)
                {
                    AddedComponentsMask = RestoreEntityMask(entId)
                };
                _entitiesData.Add(entId, replicatedEntity);
            }
        }

        private int RestoreEntityMask(in uint entityId)
        {
            var mask = 0;
            foreach (var pools in _componentHashToPool)
            {
                if (pools.Value.Contains(entityId))
                {
                    mask |= pools.Value.Mask;
                }
            }

            return mask;
        }

        private void DeserializeComponentPools(ISerializePacker serializePacker)
        {
            var poolSize = serializePacker.ReadInt();
            if (poolSize > _poolsLength)
            {
                ResizeComponentPools(poolSize);
            }
            
            for (int i = 0; i < _poolsLength; i++)
            {
                if (!serializePacker.ReadBool())
                    continue;
                _pools[i].Deserialize(serializePacker);
            }
        }

        //TODO Merge from pools
        public void MergeTo(EcsState other)
        {
            ProcessRemoved();
            other.ProcessRemoved();
            MergeEntitiesTo(other._entitiesData);
        }
    }
}