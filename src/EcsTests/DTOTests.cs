using EcsCore;
using EcsCore.Serialization;
using EcsCore.Serialization.DataContainer;
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

        private EcsState _firstState;
        private EcsState _secondState;
        private BitSerializePacker _serialzePacker;

        [SetUp]
        public void Setup()
        {
            _firstState = new EcsState();
            _secondState = new EcsState();
            _serialzePacker = new BitSerializePacker();
           
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
            _firstState.ProcessRemoved();
            _secondState.ProcessRemoved();
        }

        [Test]
        public void SerDeserTest()
        {
            var newBuffer = new byte[1024];
            _serialzePacker.SetBuffer(ref newBuffer);
            _component.Serialize(_serialzePacker);
            var firstPos = _serialzePacker.GetStreamPosition();
            var firstBuffer = _serialzePacker.GetBuffer();
            var deseredComponent = new TestComponent();
            
            _serialzePacker.SetBuffer(ref firstBuffer);
            // _serialzePacker.SetStream(new MemoryStream(firstBuffer));
            deseredComponent.Deserialize(_serialzePacker);
            // _serialzePacker.SetStream(new MemoryStream());
            deseredComponent.Serialize(_serialzePacker);
            
            var secondPos = _serialzePacker.GetStreamPosition();
            var secondBuffer = _serialzePacker.GetBuffer();

            for (int i = 0; i < firstPos; i++)
            {
                if(firstBuffer[i] != secondBuffer[i])
                    Assert.Fail("Data mismatch");
            }
            
            Assert.Pass();
        }
        
        // [Test]
        // public void SerDiffDeserDiffTest()
        // {
        //     var stream = new MemoryStream();
        //     _serialzePacker.SetStream(stream);
        //
        //     _firstDiffData.SerializeDiffable(_serialzePacker, _secondDiffData);
        //     var firstPos = _serialzePacker.GetStreamPosition();
        //     var firstBuffer = _serialzePacker.GetBuffer();
        //     
        //     var deseredData = new TestSerializeDiffData();
        //     
        //     _serialzePacker.SetStream(new MemoryStream(firstBuffer));
        //     deseredData.DeserializeDiffable(_serialzePacker, _secondDiffData);
        //     _serialzePacker.SetStream(new MemoryStream());
        //     deseredData.SerializeDiffable(_serialzePacker, _secondDiffData);
        //     
        //     var secondPos = _serialzePacker.GetStreamPosition();
        //     var secondBuffer = _serialzePacker.GetBuffer();
        //
        //     for (int i = 0; i < firstPos; i++)
        //     {
        //         if(firstBuffer[i] != secondBuffer[i])
        //             Assert.Fail("Data mismatch");
        //     }
        //     
        //     Assert.Pass();
        // }
        //
        // [Test]
        // public void EcsSerTest()
        // {
        //     var stream = new MemoryStream();
        //     _serialzePacker.SetStream(stream);
        //
        //     _firstState.Serialize(_serialzePacker);
        //     var firstPos = _serialzePacker.GetStreamPosition();
        //     var firstBuffer = _serialzePacker.GetBuffer();
        //     
        //     _serialzePacker.SetStream(new MemoryStream(firstBuffer));
        //     _secondState.Deserialize(_serialzePacker);
        //     _serialzePacker.SetStream(new MemoryStream());
        //     _secondState.Serialize(_serialzePacker);
        //     
        //     var secondPos = _serialzePacker.GetStreamPosition();
        //     var secondBuffer = _serialzePacker.GetBuffer();
        //
        //     for (int i = 0; i < firstPos; i++)
        //     {
        //         if(firstBuffer[i] != secondBuffer[i])
        //             Assert.Fail("Data mismatch");
        //     }
        //     
        //     Assert.Pass();
        // }
    }
}