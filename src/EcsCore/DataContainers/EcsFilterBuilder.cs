using System.Runtime.CompilerServices;
using EcsCore.Components;
using EcsCore.Network;

namespace EcsCore
{
    public static class EcsFilterBuilder
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static EcsFilter Include<T>(this EcsFilter filter) where T : struct, IComponentData
        {
            var newComponentMask = filter.ComponentMask.Include<T>();
            ref var componentMask = ref filter.ComponentMask;
            componentMask = newComponentMask;
            return filter;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static EcsFilter Exclude<T>(this EcsFilter filter) where T : struct, IComponentData
        {
            var newComponentMask = filter.ComponentMask.Exclude<T>();
            ref var componentMask = ref filter.ComponentMask;
            componentMask = newComponentMask;
            return filter;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static EcsFilter IncludeAny(this EcsFilter filter)
        {
            var newComponentMask = filter.ComponentMask.IncludeAny();
            ref var componentMask = ref filter.ComponentMask;
            componentMask = newComponentMask;
            return filter;
        }
       
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static EcsFilter ExcludeAny(this EcsFilter filter)
        {
            var newComponentMask = filter.ComponentMask.ExcludeAny();
            ref var componentMask = ref filter.ComponentMask;
            componentMask = newComponentMask;
            return filter;
        }
    }
}