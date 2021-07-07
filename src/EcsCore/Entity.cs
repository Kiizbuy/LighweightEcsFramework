using EcsCore.Components;
using NetCodeUtils;

namespace EcsCore
{
    public readonly struct Entity
    {
        private readonly uint _entityId;
        private readonly EcsState _ecsState;
        
        public Entity(EcsState ecsState, uint entityId)
        {
            _ecsState = ecsState;
            _entityId = entityId;
        }

        public ref T AddComponent<T>() where T : struct, IComponentData, ISerializableData
        {
            return ref _ecsState.GetPool<T>().Add(_entityId, new T());
        }
        
        public ref T AddComponent<T>(T value) where T : struct, IComponentData, ISerializableData
        {
            return ref _ecsState.GetPool<T>().Add(_entityId, value);
        }

        public bool RemoveComponent<T>() where T : struct, IComponentData, ISerializableData
        {
            return _ecsState.GetPool<T>().RemoveByEntityId(_entityId);
        }

        public ref T GetComponent<T>() where T : struct, IComponentData, ISerializableData
        {
            return ref _ecsState.GetPool<T>().Get(_entityId);
        }

        public bool HasComponent<T>() where T : struct, IComponentData, ISerializableData
        {
            _ecsState.GetPool<T>().Get(_entityId, out var componentValueExist);
            return componentValueExist;
        }

        public int GetOwnerId() => _ecsState.GetOwnerId(_entityId);
    }
}