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
            _ecsState.ProcessRemoved();
        }

        [Test]
        public void CreateEntityTest()
        {
            ref var ent = ref _ecsState.CreateEntity();
            ent.AddComponent(new TestComponent()
            {
                TestValue = NewComponentValue
            });

            Assert.AreEqual(ent.HasComponent<TestComponent>(), true);
        }

        [Test]
        public void RemoveComponentFromEntityTest()
        {
            ref var ent = ref _ecsState.CreateEntity();
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
            ref var ent = ref _ecsState.CreateEntity();
            ent.AddComponent(new TestComponent()
            {
                TestValue = NewComponentValue
            });

            ref var component = ref ent.GetComponent<TestComponent>();
            component.TestValue = _random.Next();

            Assert.AreNotEqual(component.TestValue, NewComponentValue);
        }
    }
}