using EcsCore.Components;
using EcsCore.Components.Pool;
using NetCodeUtils;

namespace EcsCore.ComponentWork
{
    public abstract class EntityFilter
    {
        protected readonly EcsState State;

        protected EntityFilter(EcsState state)
        {
            State = state;
        }
        
        public abstract Entity Get(uint entityId);
    }

    public class EntityFilter<T> : EntityFilter where T : struct, IComponentData, ISerializableData
    {
        private readonly IComponentPool<T> _pool;
        
        public EntityFilter(EcsState state) : base(state)
        {
            _pool = state.GetPool<T>();
        }

        public EntityIdEnumerable FilteredIds()
        {
            return new EntityIdEnumerable(_pool);
        }

        public override Entity Get(uint entityId)
        {
            return State.GetEntity(entityId);
        }
        
        public readonly struct EntityIdEnumerable
        {
            private readonly IComponentPool<T> _pool;

            public EntityIdEnumerable(IComponentPool<T> pool)
            {
                _pool = pool;
            }

            public EntityIdEnumerator GetEnumerator()
            {
                return new EntityIdEnumerator(_pool.GetArrayEnumerableByRef().GetEnumerator());
            }

            public struct EntityIdEnumerator
            {
                private ComponentPool<T>.StructEnumerator _enumerator;

                public EntityIdEnumerator(ComponentPool<T>.StructEnumerator enumerator)
                {
                    _enumerator = enumerator;
                }

                public bool MoveNext() => _enumerator.MoveNext();
                public void Reset() => _enumerator.Reset();
                public uint Current => _enumerator.CurrentEntityId;
            }
        }
    }
}