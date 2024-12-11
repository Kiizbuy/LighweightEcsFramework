using EcsCore.Components;
using EcsCore.Network;

namespace EcsCore.Serialization.Resolvers
{
    public interface IComponentResolver<TComponent> : IComponentResolver
        where TComponent : struct, IComponentData
    {
        void Serialize(ref TComponent component, ISerializePacker packer);
        void Deserialize(ISerializePacker packer, ref TComponent component);
    }
}