using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edument.CQRS;
using Events.Something;

namespace YourDomain.Something
{
    public class SomethingAggregate : Aggregate,
        IApplyEvent<SomethingHappened>
    {
        public bool AlreadyHappened { get; private set; }

        public void Apply(SomethingHappened e)
        {
            AlreadyHappened = true;
        }
    }
}
