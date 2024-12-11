using EcsCore.Components;

namespace EcsCore.Serialization.Resolvers
{
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
}