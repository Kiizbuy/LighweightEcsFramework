using System;
using EcsCore;
using NUnit.Framework;

namespace EcsTests
{
    [TestFixture]
    public class FilterTests
    {
        private EcsFilter _filter;
        private EcsState _state;

        [SetUp]
        public void Setup()
        {
            _filter = new EcsFilter()
                .Include<TestComponent>()
                .Include<TestComponentData>()
                .IncludeAny()
                .Exclude<ExcludeComponent>();
            _state = new EcsState();
        }

        [TearDown]
        public void TearDown()
        {
            _state.Clear();
            _state.ProcessRemoved();
        }

        [Test]
        public void ForeachTest()
        {
            ref var firstEntity = ref _state.CreateEntity();
            ref var secondEntity = ref _state.CreateEntity();
            ref var thirdEntity = ref _state.CreateEntity();
            int summaryEntityCount = 0;

            firstEntity.AddComponent(new TestComponent()
            {
                TestValue = 3
            });
            firstEntity.AddComponent(new TestComponentData()
            {
                TestValue = 3
            });
            firstEntity.AddComponent<ExcludeComponent>();

            secondEntity.AddComponent(new TestComponent()
            {
                TestValue = 2
            });

            secondEntity.AddComponent(new TestComponentData()
            {
                TestValue = 6
            });


            thirdEntity.AddComponent(new TestComponent()
            {
                TestValue = 3
            });
            foreach (ref var filteredEntity in _filter.GetFilteredEntitiesByRefFrom(_state))
            {
                summaryEntityCount++;
                ref var newEntity = ref _state.CreateEntity();
                newEntity.AddComponent<TestComponentData>();
                
                if (filteredEntity.HasComponent<TestComponent>())
                {
                    ref var testComponent = ref filteredEntity.GetComponent<TestComponent>();
                    testComponent.TestValue = 1;
                }

                if (filteredEntity.HasComponent<TestComponentData>())
                {
                    ref var testComponentData = ref filteredEntity.GetComponent<TestComponentData>();
                    testComponentData.TestValue = 1;
                }

                Console.WriteLine($"Filtered. summary count {summaryEntityCount}");
            }
            Assert.AreEqual(summaryEntityCount, 2);
            

            if (secondEntity.GetComponent<TestComponent>().TestValue == 1 &&
                secondEntity.GetComponent<TestComponentData>().TestValue == 1 &&
                firstEntity.GetComponent<TestComponent>().TestValue == 3 &&
                firstEntity.GetComponent<TestComponentData>().TestValue == 3)
            {
                Assert.Pass();
            }

            Assert.Fail();
        }

        [Test]
        public void ComponentMaskTest()
        {
            ref var entity = ref _state.CreateEntity();
            entity.AddComponent<TestComponentData>();
            entity.AddComponent<ExcludeComponent>();
            entity.RemoveComponent<ExcludeComponent>();
            entity.RemoveComponent<TestComponentData>();
            if (entity.HasNoComponents())
            {
                Assert.Pass();
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}