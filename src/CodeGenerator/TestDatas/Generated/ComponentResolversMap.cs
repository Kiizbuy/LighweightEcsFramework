using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using CodeGenerator.TestDatas;

namespace EcsCore.Serialization.Resolvers
{
    internal static partial class ResolversMap
    {
        private static ConcurrentDictionary<string, IComponentResolver> _componentResolvers;

        static ResolversMap()
        {
            _componentResolvers = new ConcurrentDictionary<string, IComponentResolver>();
            Initialize();
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
    
    public abstract class ComponentResolver<TComponent, TData> : IComponentResolver<TComponent>
        where TComponent : struct, IComponentData
        where TData : struct, ISerializableData
    {
        protected abstract TData FillSerializableDataFrom(ref TComponent component);
        protected abstract void FillComponent(ref TData data, ref TComponent component);

        public void Serialize(ref TComponent component, ISerializePacker packer)
        {
            FillSerializableDataFrom(ref component).Serialize(packer);
        }

        public void Deserialize(ISerializePacker packer, ref TComponent component)
        {
            var data = new TData();
            data.Deserialize(packer);
            FillComponent(ref data, ref component);
        }
    }
    
    public interface IComponentResolver
    {
    }
    
    public interface IComponentResolver<TComponent> : IComponentResolver
        where TComponent : struct, IComponentData
    {
        void Serialize(ref TComponent component, ISerializePacker packer);
        void Deserialize(ISerializePacker packer, ref TComponent component);
    }
}