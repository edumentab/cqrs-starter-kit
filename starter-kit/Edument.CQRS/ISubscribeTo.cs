using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Edument.CQRS
{
    /// <summary>
    /// Implemented by anything that wishes to subscribe to an event emitted by
    /// an aggregate and successfully stored.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface ISubscribeTo<TEvent>
    {
        void Handle(TEvent e);
    }
}
