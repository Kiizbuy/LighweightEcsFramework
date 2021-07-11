using System;
using System.Collections.Generic;
using System.Diagnostics;
using EcsCore;
using NUnit.Framework;

namespace EcsTests
{
    [TestFixture]
    public class EcsStateTests
    {
        private EcsState _ecsState;
        private Random _random;

        private const uint EntitySpaceSize = 1000;
        private const int NewComponentValue = 1000;
        private const int MinIterations = 1;
        private const int MaxIterations = 5;
        private const int MaxEntitiesCount = 10000;

        [SetUp]
        public void Setup()
        {
            _ecsState = new EcsState().SetupEntitiesPerSpace(EntitySpaceSize);
            _random = new Random();
        }

        [TearDown]
        public void TearDown()
        {
            _ecsState.Clear();
        }

        [Test]
        public void CreateEntityTest()
        {
            var ent = _ecsState.CreateEntity();
            ent.AddComponent(new TestComponent()
            {
                TestValue = NewComponentValue
            });

            Assert.AreEqual(ent.HasComponent<TestComponent>(), true);
        }

        [Test]
        public void RemoveComponentFromEntityTest()
        {
            var ent = _ecsState.CreateEntity();
            ent.AddComponent(new TestComponent()
            {
                TestValue = NewComponentValue
            });

            ent.RemoveComponent<TestComponent>();

            Assert.AreEqual(ent.HasComponent<TestComponent>(), false);
        }

        [Test]
        public void RefComponentValueChangeByEntityTest()
        {
            var ent = _ecsState.CreateEntity();
            ent.AddComponent(new TestComponent()
            {
                TestValue = NewComponentValue
            });

            ref var component = ref ent.GetComponent<TestComponent>();
            component.TestValue = _random.Next();

            Assert.AreNotEqual(component.TestValue, NewComponentValue);
        }

        [Test]
        public void FilterWorkTest()
        {
            CreateEntityAndAddTestComponent(_random.Next(MinIterations, MaxIterations));

            try
            {
                IterateEntitiesByFilter();
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
                throw;
            }
        }

        private void IterateEntitiesByChangedFilter()
        {
            var filter = _ecsState.GetFilteredEntitiesByComponent<TestComponent>();
            foreach (var entId in filter.FilteredIds())
            {
                ref var component = ref filter.Get(entId).GetComponent<TestComponent>();
                component.TestValue = 0;
            }
        }

        [Test]
        public void EcsStateWorkTimeBenchmark()
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            CreateEntityAndAddTestComponent(MaxEntitiesCount);
            stopwatch.Stop();
            var elapsedCreateEntityTime = stopwatch.ElapsedMilliseconds;
            
            stopwatch.Start();
            IterateEntitiesByChangedFilter();
            stopwatch.Stop();
            var elapsedChangedFilterWorkTime = stopwatch.ElapsedMilliseconds;
            
            stopwatch.Start();
            IterateEntitiesByFilter();
            stopwatch.Stop();
            var elapsedFilterWorkTime = stopwatch.ElapsedMilliseconds;

            Assert.Pass(
                $"Entities time creation: {elapsedCreateEntityTime} ms | default filter time work: {elapsedFilterWorkTime} ms | changed filter time work {elapsedChangedFilterWorkTime}");
        }

        private void IterateEntitiesByFilter()
        {
            foreach (var ent in _ecsState.GetFilteredEntities<TestComponent>())
            {
                ref var component = ref ent.GetComponent<TestComponent>();
                component.TestValue = 0;
            }
        }

        private void CreateEntityAndAddTestComponent(int entitiesCount)
        {
            for (int i = 0; i < entitiesCount; i++)
            {
                _ecsState.CreateEntity().AddComponent(new TestComponent()
                {
                    TestValue = _random.Next()
                });
            }
        }
    }
}