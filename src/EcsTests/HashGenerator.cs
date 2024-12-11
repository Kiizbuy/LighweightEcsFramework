using System;
using EcsCore;
using NUnit.Framework;

namespace EcsTests
{
    [TestFixture]
    public class HashGenerator
    {
        [Test]
        public void Test()
        {
            var huy = "TransformComponent";
            var manda = "Resolver";
            var oo = "Component2Fix";
            var oou = "Component2Fix12";

            Console.WriteLine(huy.GenerateHash());
            Console.WriteLine(manda.GenerateHash());
            Console.WriteLine(oo.GenerateHash());
            Console.WriteLine(oou.GenerateHash());

            
            Assert.True(huy.GenerateHash() % 2 == 0);
            Assert.True(manda.GenerateHash() % 2 == 0);
            Assert.True(oo.GenerateHash() % 2 == 0);

        }
    }
}