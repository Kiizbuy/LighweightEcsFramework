using System;
using System.IO;
using EcsCore;
using NetCodeUtils;
using NUnit.Framework;

namespace EcsTests
{
    [TestFixture]
    public class DTOTests
    {
        private const int ComponentValue = 15;

        private TestComponent _component;
        private TestSerializeDiffData _firstDiffData;
        private TestSerializeDiffData _secondDiffData;

        private Random _random;
        private EcsState _firstState;
        private EcsState _secondState;
        private IPacker _packer;

        [SetUp]
        public void Setup()
        {
            _firstState = new EcsState();
            _secondState = new EcsState();
            _packer = new BitsPacker();
            _random = new Random();
           
            _component = new TestComponent
            {
                TestValue = ComponentValue
            };
            
            _firstDiffData = new TestSerializeDiffData
            {
                Min = 11,
                Max = 12,
                Velocity = 13
            };

            _secondDiffData = new TestSerializeDiffData
            {
                Min = 11,
                Max = 11,
                Velocity = 13
            };

            _firstState.CreateEntity().AddComponent(new TestComponent()
            {
                TestValue = 15
            });
            
            _firstState.CreateEntity().AddComponent(new TestComponent()
            {
                TestValue = 15
            });
            
            //TODO Add All component types pool generation
            _secondState.CreateEntity().AddComponent(new TestComponent()
            {
                TestValue = 21
            });
        }

        [TearDown]
        public void TearDown()
        {
            _firstState.Clear();
            _secondState.Clear();
        }

        [Test]
        public void SerDeserTest()
        {
            var stream = new MemoryStream();
            _packer.SetStream(stream);

            _component.Serialize(_packer);
            var firstPos = _packer.GetStreamPosition();
            var firstBuffer = _packer.Flush();
            var deseredComponent = new TestComponent();
            
            _packer.SetStream(new MemoryStream(firstBuffer));
            deseredComponent.Deserialize(_packer);
            _packer.SetStream(new MemoryStream());
            deseredComponent.Serialize(_packer);
            
            var secondPos = _packer.GetStreamPosition();
            var secondBuffer = _packer.Flush();

            for (int i = 0; i < firstPos; i++)
            {
                if(firstBuffer[i] != secondBuffer[i])
                    Assert.Fail("Data mismatch");
            }
            
            Assert.Pass();
        }
        
        [Test]
        public void SerDiffDeserDiffTest()
        {
            var stream = new MemoryStream();
            _packer.SetStream(stream);

            _firstDiffData.SerializeDiffable(_packer, _secondDiffData);
            var firstPos = _packer.GetStreamPosition();
            var firstBuffer = _packer.Flush();
            
            var deseredData = new TestSerializeDiffData();
            
            _packer.SetStream(new MemoryStream(firstBuffer));
            deseredData.DeserializeDiffable(_packer, _secondDiffData);
            _packer.SetStream(new MemoryStream());
            deseredData.SerializeDiffable(_packer, _secondDiffData);
            
            var secondPos = _packer.GetStreamPosition();
            var secondBuffer = _packer.Flush();

            for (int i = 0; i < firstPos; i++)
            {
                if(firstBuffer[i] != secondBuffer[i])
                    Assert.Fail("Data mismatch");
            }
            
            Assert.Pass();
        }
        
        [Test]
        public void EcsSerTest()
        {
            var stream = new MemoryStream();
            _packer.SetStream(stream);

            _firstState.Serialize(_packer);
            var firstPos = _packer.GetStreamPosition();
            var firstBuffer = _packer.Flush();
            
            _packer.SetStream(new MemoryStream(firstBuffer));
            _secondState.Deserialize(_packer);
            _packer.SetStream(new MemoryStream());
            _secondState.Serialize(_packer);
            
            var secondPos = _packer.GetStreamPosition();
            var secondBuffer = _packer.Flush();

            for (int i = 0; i < firstPos; i++)
            {
                if(firstBuffer[i] != secondBuffer[i])
                    Assert.Fail("Data mismatch");
            }
            
            Assert.Pass();
        }
    }
}