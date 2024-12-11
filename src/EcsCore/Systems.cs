namespace EcsCore
{
    public interface IEcsSystem
    {
    }

    public interface IEcsInitSystem : IEcsSystem
    {
        void Init();
    }

    public interface IEcsRunOnceSystem : IEcsSystem
    {
        void Run(EcsState current);
    }
    
    public interface IEcsRunOnceViewSystem : IEcsSystem
    {
        void Run(EcsState current);
    }

    public interface IEcsViewSystem : IEcsSystem
    {
        void Update(EcsState previous, EcsState current, float delta, float tickPercentage);
    }

    public interface IEcsSimulationSystem : IEcsSystem
    {
        void Simulate(EcsState current, uint simTick);
    }
}