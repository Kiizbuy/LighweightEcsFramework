using System;
using System.Collections.Generic;
using System.Linq;
using EcsCore.Components;

namespace EcsCore
{
    public sealed class World
    {
        public event Action SimulationEnded;
        public event Action SimulationBegin;
        
        private readonly ISet<IEcsRunOnceSystem> _allRunOnceSystems = new HashSet<IEcsRunOnceSystem>();
        private readonly ISet<IEcsRunOnceViewSystem> _allRunOnceViewSystems = new HashSet<IEcsRunOnceViewSystem>();
        private readonly ISet<IEcsSimulationSystem> _allSimulationSystems = new HashSet<IEcsSimulationSystem>();
        private readonly ISet<IEcsViewSystem> _allViewSystems = new HashSet<IEcsViewSystem>();

        private EcsState _current;

        private CircularBuffer<EcsState> _statesBuffer;

        public EcsState CurrentSimulating => _current;
        public EcsState LastSimulated => _statesBuffer.Current;


        private uint _simTick;
        private uint _renderTick;

        public World (int maxStateBufferSize, Func<EcsState> createState)
        {
            _statesBuffer = new CircularBuffer<EcsState>();

            var buffer = new EcsState[maxStateBufferSize];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = createState();
            }

            _statesBuffer.SetBuffer(buffer);

            _current = _statesBuffer.GetNext();
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
            var prev = _statesBuffer.Current;
            var next = _statesBuffer.GetNext();

            if (prev != next)
            {
                prev.CopyTo(next);
            }

#if DEBUG
            _current.Unlock();
#endif

            _current.LocalTick = prev.LocalTick + 1;
            _current.SimTick = prev.SimTick + 1;

            foreach (var runOnce in _allRunOnceSystems)
            {
                runOnce.Run(_current);
            }

            _allRunOnceSystems.Clear();

            foreach (var simulationSystem in _allSimulationSystems)
            {
                simulationSystem.Simulate(_current, _current.SimTick);
            }

            _current.ProcessRemoved();
            var nextIndex = _statesBuffer.GetNextIndex();
            var temp = _statesBuffer.GetBuffer[nextIndex];
            _statesBuffer.GetBuffer[nextIndex] = _current;
#if DEBUG
            _current.Lock();
#endif
            _current = temp;
            _statesBuffer.GetNext();
        }

        public void UpdateView(EcsState current, EcsState previous, float delta, float tickPercentage)
        {
            if (_allRunOnceViewSystems.Count > 0)
            {
                foreach (var sys in _allRunOnceSystems)
                {
                    sys.Run(current);
                }

                _allRunOnceViewSystems.Clear();
            }

            foreach (var viewSystem in _allViewSystems)
            {
                viewSystem.Update(current, previous, delta, tickPercentage);
            }
        }
    }
}