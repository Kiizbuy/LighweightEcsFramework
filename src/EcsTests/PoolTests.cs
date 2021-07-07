using System;
using EcsCore.Components.Pool;
using NUnit.Framework;

namespace EcsTests
{
    [TestFixture]
    public class PoolTests
    {
        private const uint TestEntityId = 1;
        
        private IComponentPool<TestComponent> _pool;
        private Random _random;

        private const int MinIterations = 3;
        private const int MaxIterations = 5;
        
        [SetUp]
        public void Setup()
        {
            _pool = new ComponentPool<TestComponent>();
            _random = new Random();
        }

        [TearDown]
        public void TearDown()
        {
            _pool.Clear();
        }

        [Test]
        public void AddPass()
        {
            _pool.Add(TestEntityId, new TestComponent()
            {
                TestValue = 1
            });
            
            Assert.AreEqual(_pool.Count, 1);
        }

        [Test]
        public void RefGetPass()
        {
            const int newValue = 5;
            _pool.Add(TestEntityId, new TestComponent()
            {
                TestValue = 1
            });
            ref var component = ref _pool.Get(TestEntityId);
            component.TestValue = newValue;
            
            Assert.AreEqual(component.TestValue, newValue);
        }

        [Test]
        public void EnumerationPass()
        {
            var entitiesCount = _random.Next(MinIterations, MaxIterations);
            var entityId = 1u;
            
            for (var i = 0; i < entitiesCount; i++)
            {
                _pool.Add(++entityId, new TestComponent());
            }

            try
            {
                foreach (ref var component in _pool.GetArrayEnumerableByRef())
                {
                    component.TestValue = _random.Next();
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
                throw;
            }
            
            Assert.AreEqual(_pool.Count, entitiesCount);
        }
        
#if DEBUG
        [Test]
        public void DeleteErrorPass()
        {
            _pool.Add(TestEntityId, new TestComponent());

            try
            {
                _pool.RemoveByEntityId(0u);
            }
            catch (Exception)
            {
                Assert.Pass();
                throw;
            }
        }
#endif
    }
}