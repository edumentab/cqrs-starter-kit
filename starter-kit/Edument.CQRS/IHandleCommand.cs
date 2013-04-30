using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Edument.CQRS
{
    public interface IHandleCommand<TCommand, TAggregate>
    {
        IEnumerable Handle(Func<Guid, TAggregate> al, TCommand c);
    }
}
