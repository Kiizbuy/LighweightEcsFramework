using System.Collections.Generic;
using EcsCore.Utils;

namespace EcsCore
{
    internal class EntitySpace : ICopyable<EntitySpace>
    {
        private uint _entityId;
        
        public readonly uint Base;
        public readonly uint Size;
        private readonly SortedSet<uint> _freeEntityIds;

        public EntitySpace(uint @base, uint size)
        {
            Base = @base;
            Size = size;
            _entityId = Base;
            _freeEntityIds = new SortedSet<uint>();
        }

        public uint GetNewEntityId()
        {
            if (_freeEntityIds.Count == 0)
            {
                return _entityId++;
            }

            var entityId = _freeEntityIds.Max;
            _freeEntityIds.Remove(entityId);
            return entityId;
        }
        
        public uint GetMinEntityId()
        {
            return Base;
        }
        
        public uint GetMaxEntityId()
        {
            return Base + Size - 1;
        }
        
        public bool IsFull()
        {
            return _entityId > GetMaxEntityId() && _freeEntityIds.Count == 0;
        }
        
        public void ReleaseEntityId(uint entityId)
        {
            if (entityId < _entityId - 1)
            {
                _freeEntityIds.Add(entityId);
                return;
            }

            --_entityId;
            while (_freeEntityIds.Count != 0)
            {
                entityId = _freeEntityIds.Max;
                if (entityId < _entityId - 1)
                {
                    break;
                }

                _freeEntityIds.Remove(entityId);
                --_entityId;
            }
        }

        public void CopyTo(EntitySpace other)
        {
            other._entityId = _entityId;
            other._freeEntityIds.Clear();
            foreach (var freeEntityId in _freeEntityIds)
            {
                other._freeEntityIds.Add(freeEntityId);
            }
        }
    }
}