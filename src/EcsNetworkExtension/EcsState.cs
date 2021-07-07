using System.Runtime.Serialization;
using NetCodeUtils;
namespace EcsCore
{
    public sealed partial class EcsState : ISerializableData
    {
        
        public void Serialize(IPacker packer)
        {
        }

        public void Deserialize(IPacker packer)
        {
            throw new System.NotImplementedException();
        }
    }
}