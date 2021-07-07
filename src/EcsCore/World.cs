using System;
using System.Collections.Generic;
using System.Linq;
using EcsCore.Components;

namespace EcsCore
{
    public sealed class World
    {
        private readonly ISet<IEcsRunOnceSystem> _allRunOnceSystems = new HashSet<IEcsRunOnceSystem>();
        private readonly ISet<IEcsSimulationSystem> _allSimulationSystems = new HashSet<IEcsSimulationSystem>();
        private readonly ISet<IEcsViewSystem> _allViewSystems = new HashSet<IEcsViewSystem>();
        
        private EcsState _current;
        private EcsState _previous;

        private CircularBuffer<EcsState> _statesBuffer;

        public EcsState CurrentSimulating => _current;
        public EcsState LastSimulated => _statesBuffer.Current;


        private uint _simTick;
        private uint _renderTick;

        public World SetupWorld(int maxStateBufferSize, Func<EcsState> createState)
        {
            _statesBuffer = new CircularBuffer<EcsState>();
            
            var buffer = new EcsState[maxStateBufferSize];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = createState();
            }
            
            _statesBuffer.SetBuffer(buffer);
            
            _current = _statesBuffer.GetNext();
            _previous = _statesBuffer.Current;
            
            return this;
        }

        public World AddSystem(IEcsSystem system)
        {
            switch (system)
            {
                case IEcsInitSystem ecsInitSystem:
                    ecsInitSystem.Init();
                    break;
                case IEcsRunOnceSystem ecsRunOnceSystem:
                    _allRunOnceSystems.Add(ecsRunOnceSystem);
                    break;
                case IEcsSimulationSystem ecsSimulationSystem:
                    _allSimulationSystems.Add(ecsSimulationSystem);
                    break;
                case IEcsViewSystem ecsViewSystem:
                    _allViewSystems.Add(ecsViewSystem);
                    break;
            }
            return this;
        }

        public World RemoveSystem(IEcsSystem system)
        {
            switch (system)
            {
                case IEcsRunOnceSystem ecsRunOnceSystem:
                    _allRunOnceSystems.Remove(ecsRunOnceSystem);
                    break;
                case IEcsSimulationSystem ecsSimulationSystem:
                    _allSimulationSystems.Remove(ecsSimulationSystem);
                    break;
                case IEcsViewSystem ecsViewSystem:
                    _allViewSystems.Remove(ecsViewSystem);
                    break;
            }
            return this;
        }

        public void Simulate()
        {
            var current = _statesBuffer.Current;
            var next = _statesBuffer.GetNext();
            
            if (current != next)
            {
                current.CopyTo(next);
            }
            
            next.Tick = current.Tick + 1;
            
            foreach (var runOnce in _allRunOnceSystems)
            {
                runOnce.Run(_current);
            }
            
            _allRunOnceSystems.Clear();
            
            foreach (var simulationSystem in _allSimulationSystems)
            {
                simulationSystem.Simulate(_current, _simTick);
            }
        }

        public void UpdateViewSystem()
        {
            foreach (var viewSystem in _allViewSystems)
            {
                viewSystem.Update(_previous, _current);
            }
        }
    }
}