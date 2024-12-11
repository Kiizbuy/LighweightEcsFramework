using Components;

namespace EcsCore.Serialization.Resolvers
{
    internal static partial class ResolversMap
    {
        static partial void Initialize()
        {
            //Codegen part Here
            _componentResolvers.TryAdd(nameof(TestDataComponentData), new TestDataComponentResolver());
        }
    }
}