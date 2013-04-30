using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using YourDomain.Something;
using Edument.CQRS;
using Events.Something;

namespace YourDomainTests
{
    [TestFixture]
    public class SomethingTests : BDDTest<SomethingCommandHandlers, SomethingAggregate>
    {
        private Guid testId;

        [SetUp]
        public void Setup()
        {
            testId = Guid.NewGuid();
        }

        [Test]
        public void SomethingCanHappen()
        {
            Test(
                Given(),
                When(new MakeSomethingHappen
                {
                    Id = testId,
                    What = "Boom!"
                }),
                Then(new SomethingHappened
                {
                    Id = testId,
                    What = "Boom!"
                }));
        }

        [Test]
        public void SomethingCanHappenOnlyOnce()
        {
            Test(
                Given(new SomethingHappened
                {
                    Id = testId,
                    What = "Boom!"
                }),
                When(new MakeSomethingHappen
                {
                    Id = testId,
                    What = "Boom!"
                }),
                ThenFailWith<SomethingCanOnlyHappenOnce>());
        }
    }
}
