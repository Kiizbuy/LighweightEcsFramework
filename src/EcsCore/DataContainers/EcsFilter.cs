using System.Runtime.CompilerServices;

namespace EcsCore
{
    public class EcsFilter
    {
        private ComponentMask _componentMask;

        internal ref ComponentMask ComponentMask
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _componentMask;
        }

        public FilteredEntityEnumerableByRef GetFilteredEntitiesByRefFrom(EcsState state)
        {
            return new FilteredEntityEnumerableByRef(_componentMask, state);
        }
    }
}