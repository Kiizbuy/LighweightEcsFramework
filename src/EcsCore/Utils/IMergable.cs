namespace EcsCore.Utils
{
    public interface IMergable<in T>
    {
        void MergeTo(T other);
    }
}