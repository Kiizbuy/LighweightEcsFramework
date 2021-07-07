using System.Threading;
using EcsCore.Components;

namespace EcsCore
{
    internal static class EcsPoolTypes<T> where T: struct, IComponentData
    {
        public static readonly int TypeIndex;

        static EcsPoolTypes()
        {
            TypeIndex = Interlocked.Increment(ref ComponentPoolCounter.ComponentTypesCount);
        }
    }
    
    internal sealed class ComponentPoolCounter
    {
        internal static int ComponentTypesCount;
    }
}