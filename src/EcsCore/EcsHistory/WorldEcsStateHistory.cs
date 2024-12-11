using System;

namespace EcsCore
{
    public class WorldStatesSnapshot
    {
        public EcsState Current;
        public EcsState Previous;
    }
    
    public class WorldEcsStateHistory : IDisposable
    {
        private World _world;
        private Sampler<WorldStatesSnapshot> _history;

        public WorldEcsStateHistory(World world)
        {
            _history = new Sampler<WorldStatesSnapshot>();
            _world = world;
            _world.SimulationEnded += CreateSample;
        }

        private void CreateSample()
        {
            CreateSample(_world.LastSimulated, _world.CurrentSimulating, _world.LastSimulated.SimTick);
        }
        
        public void CreateSample(EcsState lastSimulated, EcsState currentSimulating, uint tick)
        {
            var newSample = _history.CreateSample(tick);
            var currentSimulated = new EcsState();
            currentSimulating.CopyTo(currentSimulated);
            newSample.Data.Current = currentSimulated;
            newSample.Data.Previous = lastSimulated;
        }

        public void Init(int maxBufferSize)
        {
            _history.Init(maxBufferSize, 0);
        }

        public void RevertToTick(World world, uint tick)
        {
            var sample = _history.GetSample(tick);
            
            
        }

        public void Dispose()
        {
            _world.SimulationEnded -= CreateSample;
        }
    }
}