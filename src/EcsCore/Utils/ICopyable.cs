namespace EcsCore.Utils
{
    public interface ICopyable<in T>
    {
        void CopyTo(T other);
    }
}