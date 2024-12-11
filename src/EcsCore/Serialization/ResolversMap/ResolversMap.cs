using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using EcsCore.Components;

namespace EcsCore.Serialization.Resolvers
{
    internal static partial class ResolversMap
    {
        private static ConcurrentDictionary<string, IComponentResolver> _componentResolvers;

        static ResolversMap()
        {
            _componentResolvers = new ConcurrentDictionary<string, IComponentResolver>();
            Initialize();
            // _componentResolvers.TryAdd(nameof(TestDataComponent), new TestDataComponentResolver());
        }

        static partial void Initialize();

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.Synchronized)]
        internal static IComponentResolver<TComponent> GetResolver<TComponent>()
            where TComponent : struct, IComponentData
        {
            return GetResolver<TComponent>(nameof(TComponent));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.Synchronized)]
        private static IComponentResolver<TComponent> GetResolver<TComponent>(string typeName)
            where TComponent : struct, IComponentData
        {
            return (IComponentResolver<TComponent>)_componentResolvers[typeName];
        }
    }
}