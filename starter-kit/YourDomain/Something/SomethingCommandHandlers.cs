using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edument.CQRS;
using System.Collections;
using Events.Something;

namespace YourDomain.Something
{
    public class SomethingCommandHandlers :
        IHandleCommand<MakeSomethingHappen, SomethingAggregate>
    {
        public IEnumerable Handle(Func<Guid, SomethingAggregate> al, MakeSomethingHappen c)
        {
            var agg = al(c.Id);

            if (agg.AlreadyHappened)
                throw new SomethingCanOnlyHappenOnce();
            
            yield return new SomethingHappened
            {
                Id = c.Id,
                What = c.What
            };
        }
    }
}
