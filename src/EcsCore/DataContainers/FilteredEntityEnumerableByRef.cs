using EcsCore.Containers;

namespace EcsCore
{
    public readonly struct FilteredEntityEnumerableByRef
    {
        private readonly DataPool<uint, Entity>.ArrayEnumerableByRef _entityEnumerable;
        private readonly ComponentMask _componentMask;

        public FilteredEntityEnumerableByRef(ComponentMask componentMask, EcsState state)
        {
            _entityEnumerable = DataPool<uint, Entity>.ArrayExtensions.ToEnumerableByRef(state.GetEntityDataPool);
            _componentMask = componentMask;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_componentMask, _entityEnumerable);
        }

        public struct Enumerator
        {
            private ComponentMask _componentMask;
            private DataPool<uint, Entity>.ArrayEnumerableByRef.Enumerator _entitiesEnumerable;

            public Enumerator(ComponentMask componentMask, DataPool<uint, Entity>.ArrayEnumerableByRef enumerableByRef)
            {
                _componentMask = componentMask;
                _entitiesEnumerable = enumerableByRef.GetEnumerator();
            }

            public ref Entity Current => ref _entitiesEnumerable.Current;

            public bool MoveNext() => MoveNext(ref this, ref _entitiesEnumerable);

            public void Reset() => Reset(ref _entitiesEnumerable);

            private static bool MoveNext(ref Enumerator enumerator,
                ref DataPool<uint, Entity>.ArrayEnumerableByRef.Enumerator entitiesEnumerable)
            {
                bool flag;
                while (true)
                {
                    if (!entitiesEnumerable.MoveNext())
                    {
                        flag = false;
                        break;
                    }
                    else
                    {
                        ref var currentEntity = ref entitiesEnumerable.Current;
                        if ((enumerator._componentMask.IsIncluded(currentEntity) &&
                             !enumerator._componentMask.IsExcluded(currentEntity)) == false)
                        {
                            continue;
                        }

                        flag = true;
                        break;
                    }
                }
                return flag;
            }

            private static void Reset(ref DataPool<uint, Entity>.ArrayEnumerableByRef.Enumerator entitiesEnumerable)
            {
                entitiesEnumerable.Reset();
            }
        }
    }
}